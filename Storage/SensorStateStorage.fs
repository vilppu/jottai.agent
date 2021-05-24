namespace Jottai

module SensorStateStorage =
    open System.Linq
    open System.Threading
    open MongoDB.Driver
    let sensorNameSemaphore = new SemaphoreSlim(1)
    let sensorStateSemaphore = new SemaphoreSlim(1) 

    let private SensorsCollectionName = "Sensors"

    let private SensorsCollection = 
        BsonStorage.Database.GetCollection<SensorState> SensorsCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"
    
    let private GetSensorExpression (deviceGroupId : DeviceGroupId) (propertyId : PropertyId) =
        let expr = Expressions.Lambda.Create<SensorState>(fun x -> x.DeviceGroupId = deviceGroupId && x.PropertyId = propertyId)
        expr
    
    let private GetSensorsExpression (deviceGroupId : DeviceGroupId) =  
        let expr = Expressions.Lambda.Create<SensorState>(fun x -> x.DeviceGroupId = deviceGroupId)
        expr

    let StoreSensorName (deviceGroupId : DeviceGroupId) (propertyId : PropertyId) (propertyName : PropertyName) =
        
        let filter = GetSensorExpression deviceGroupId propertyId
        let update = Builders<SensorState>.Update.Set((fun s -> s.PropertyName), propertyName)

        async {
            do! sensorNameSemaphore.WaitAsync() |> Async.AwaitTask

            try
                do! SensorsCollection.UpdateOneAsync<SensorState>(filter, update)
                    |> Async.AwaitTask
                    |> Async.Ignore
                    
            finally
                sensorNameSemaphore.Release() |> ignore
        }

    let StoreSensorState (sensorState : SensorState) =

        let filter = GetSensorExpression sensorState.DeviceGroupId sensorState.PropertyId
            
        async {
            do! sensorStateSemaphore.WaitAsync() |> Async.AwaitTask

            try            
                try
                    do! SensorsCollection.ReplaceOneAsync<SensorState>(filter, sensorState, BsonStorage.Replace)
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
                SensorsCollection.Find<SensorState>(filter).ToListAsync()
                |> Async.AwaitTask               
            

            let sensorStates =
                result |> Seq.map(fun x -> 
                let hasChanged = update.Measurement <> x.Measurement
                let lastActive = update.Timestamp
                let lastUpdated =
                    if hasChanged
                    then lastActive
                    else x.LastUpdated

                { DeviceGroupId = update.DeviceGroupId
                  GatewayId = update.GatewayId
                  PropertyId = update.PropertyId              
                  DeviceId = update.DeviceId
                  PropertyName = x.PropertyName
                  PropertyDescription = x.PropertyDescription
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
                SensorsCollection.Find<SensorState>(filter).ToListAsync()
                |> Async.AwaitTask

            return result |> List.ofSeq
        }
        
    let Drop() =
        BsonStorage.Database.DropCollection(SensorsCollectionName)
