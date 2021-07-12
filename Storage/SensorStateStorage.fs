namespace Jottai

module SensorStateStorage =
    open System
    open System.Linq
    open System.Threading
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    open MongoDB.Bson
    open MongoDB.Bson.Serialization.Attributes
    open MongoDB.Driver    
    
    [<CLIMutable>]
    [<BsonIgnoreExtraElements>]
    type StorableSensorState = 
        { [<BsonIgnoreIfDefault>]
          mutable Id : ObjectId
          mutable DeviceGroupId : string
          mutable GatewayId : string
          mutable DeviceId : string          
          mutable PropertyId : string
          mutable PropertyName : string
          mutable PropertyDescription : string
          mutable PropertyType : string
          mutable PropertyValue : obj
          mutable Protocol : string
          mutable BatteryVoltage : float
          mutable SignalStrength : float
          mutable LastUpdated : DateTimeOffset
          mutable LastActive : DateTimeOffset }

    let private StorableSensorState (sensorState : SensorState)
        : StorableSensorState =
        let (DeviceGroupId deviceGroupId) = sensorState.DeviceGroupId
        let (GatewayId gatewayId) = sensorState.GatewayId
        let (DeviceId deviceId) = sensorState.DeviceId
        let (PropertyId propertyId) = sensorState.PropertyId
        let (PropertyName propertyName) = sensorState.PropertyName
        let (PropertyDescription propertyDescription) = sensorState.PropertyDescription
        let propertyType = sensorState.Measurement |> Measurement.Name
        let propertyValue = sensorState.Measurement |> Measurement.Value
        let protocol = 
            match sensorState.Protocol with
               | ZWave -> "ZWave"
               | ZWavePlus -> "ZWavePlus"
               | _ -> "NotSpecified"
        let batteryVoltage = float(sensorState.BatteryVoltage)
        let signalStrength = sensorState.SignalStrength
        let lastUpdated = sensorState.LastUpdated
        let lastActive = sensorState.LastActive     

        { Id = new ObjectId()
          DeviceGroupId = deviceGroupId
          GatewayId = gatewayId
          DeviceId = deviceId
          PropertyId = propertyId
          PropertyName = propertyName
          PropertyDescription = propertyDescription
          PropertyType = propertyType
          PropertyValue = propertyValue
          Protocol = protocol
          BatteryVoltage = batteryVoltage
          SignalStrength = signalStrength
          LastUpdated = lastUpdated
          LastActive = lastActive}

    let private SensorState (storableSensorState : StorableSensorState)
        : SensorState =
        
        let protocol = 
            match storableSensorState.Protocol with
               | "ZWave" -> ZWave
               | "ZWavePlus" -> ZWavePlus
               | _ -> NotSpecified

        { DeviceGroupId = DeviceGroupId storableSensorState.DeviceGroupId        
          GatewayId = GatewayId storableSensorState.GatewayId
          DeviceId = DeviceId storableSensorState.DeviceId
          PropertyId = PropertyId storableSensorState.PropertyId
          PropertyName = PropertyName storableSensorState.PropertyName          
          PropertyDescription = PropertyDescription storableSensorState.PropertyDescription
          Measurement = (Measurement.From storableSensorState.PropertyType storableSensorState.PropertyValue).Value
          Protocol = protocol
          BatteryVoltage = storableSensorState.BatteryVoltage * 1.0<V>
          SignalStrength = storableSensorState.SignalStrength
          LastUpdated = storableSensorState.LastUpdated
          LastActive = storableSensorState.LastActive }

    let sensorNameSemaphore = new SemaphoreSlim(1)
    let sensorStateSemaphore = new SemaphoreSlim(1) 

    let private SensorsCollectionName = "Sensors"

    let private SensorsCollection = 
        BsonStorage.Database.GetCollection<StorableSensorState> SensorsCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"
    
    let private GetSensorExpression (deviceGroupId : DeviceGroupId) (propertyId : PropertyId) =
        let (DeviceGroupId deviceGroupId) = deviceGroupId
        let (PropertyId propertyId) = propertyId
        let expr = Expressions.Lambda.Create<StorableSensorState>(fun x -> x.DeviceGroupId = deviceGroupId && x.PropertyId = propertyId)
        expr
    
    let private GetSensorsExpression (deviceGroupId : DeviceGroupId) =  
        let (DeviceGroupId deviceGroupId) = deviceGroupId
        let expr = Expressions.Lambda.Create<StorableSensorState>(fun x -> x.DeviceGroupId = deviceGroupId)
        expr

    let StoreSensorName (deviceGroupId : DeviceGroupId) (propertyId : PropertyId) (propertyName : PropertyName) =
        
        let filter = GetSensorExpression deviceGroupId propertyId
        let (PropertyName propertyName) = propertyName
        let update = Builders<StorableSensorState>.Update.Set((fun s -> s.PropertyName), propertyName)

        async {
            do! sensorNameSemaphore.WaitAsync() |> Async.AwaitTask

            try
                do! SensorsCollection.UpdateOneAsync<StorableSensorState>(filter, update)
                    |> Async.AwaitTask
                    |> Async.Ignore
                    
            finally
                sensorNameSemaphore.Release() |> ignore
        }

    let StoreSensorState (sensorState : SensorState) =

        let filter = GetSensorExpression sensorState.DeviceGroupId sensorState.PropertyId
        let storable = sensorState |> StorableSensorState
        async {
            do! sensorStateSemaphore.WaitAsync() |> Async.AwaitTask

            try            
                try
                    do! SensorsCollection.ReplaceOneAsync<StorableSensorState>(filter, storable, BsonStorage.Replace)
                        :> Tasks.Task
                        |> Async.AwaitTask
                with
                | ex -> failwith ex.Message
            finally
                sensorStateSemaphore.Release() |> ignore
        }

    let GetSensorState (update : SensorStateUpdate) : Async<SensorState> =
        async {
            let filter = GetSensorExpression update.DeviceGroupId update.PropertyId
            let! result = 
                SensorsCollection.Find<StorableSensorState>(filter).ToListAsync()
                |> Async.AwaitTask               
            

            let sensorStates =
                result |> Seq.map(fun x ->
                let measurement = Measurement.From x.PropertyType x.PropertyValue
                let hasChanged = update.Measurement <> measurement.Value
                let lastActive = update.Timestamp
                let lastUpdated =
                    if hasChanged
                    then lastActive
                    else x.LastUpdated

                { DeviceGroupId = update.DeviceGroupId
                  GatewayId = update.GatewayId
                  PropertyId = update.PropertyId              
                  DeviceId = update.DeviceId
                  PropertyName = PropertyName x.PropertyName
                  PropertyDescription = PropertyDescription x.PropertyDescription
                  Measurement = update.Measurement
                  Protocol = update.Protocol
                  BatteryVoltage = update.BatteryVoltage
                  SignalStrength = update.SignalStrength
                  LastUpdated = lastUpdated
                  LastActive = lastActive })

            let initialName =
                let (DeviceId deviceId) = update.DeviceId
                let measuredProperty = update.Measurement |> Measurement.Name
                deviceId + "." + measuredProperty

            let initial = 
                { DeviceGroupId = update.DeviceGroupId
                  GatewayId = update.GatewayId
                  DeviceId = update.DeviceId
                  PropertyId = update.PropertyId
                  PropertyName = PropertyName initialName
                  PropertyDescription = PropertyDescription initialName
                  Measurement = update.Measurement
                  Protocol = update.Protocol
                  BatteryVoltage = update.BatteryVoltage
                  SignalStrength = update.SignalStrength
                  LastUpdated = update.Timestamp
                  LastActive = update.Timestamp }

            return sensorStates.DefaultIfEmpty(initial).Single()
    }

    let GetSensorStates deviceGroupId : Async<SensorState list> =
        async {
            let filter = GetSensorsExpression deviceGroupId

            let! result =
                SensorsCollection.Find<StorableSensorState>(filter).ToListAsync()
                |> Async.AwaitTask

            return result |> Seq.map SensorState |> List.ofSeq
        }
        
    let Drop() =
        BsonStorage.Database.DropCollection(SensorsCollectionName)
