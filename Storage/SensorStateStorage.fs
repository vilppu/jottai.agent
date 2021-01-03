namespace Jottai

module SensorStateStorage =
    open System
    open System.Threading
    open MongoDB.Bson
    open MongoDB.Bson.Serialization.Attributes
    open MongoDB.Driver
    let sensorNameSemaphore = new SemaphoreSlim(1)
    let sensorStateSemaphore = new SemaphoreSlim(1)
    
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

    let private SensorsCollectionName = "Sensors"

    let private SensorsCollection = 
        BsonStorage.Database.GetCollection<StorableSensorState> SensorsCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"
    
    let private GetSensorExpression (deviceGroupId : string) (propertyId : string) =
        let propertyId = propertyId
        let deviceGroupId = deviceGroupId
        let expr = Expressions.Lambda.Create<StorableSensorState>(fun x -> x.DeviceGroupId = deviceGroupId && x.PropertyId = propertyId)
        expr
    
    let private GetSensorsExpression (deviceGroupId : string) =        
        let deviceGroupId = deviceGroupId
        let expr = Expressions.Lambda.Create<StorableSensorState>(fun x -> x.DeviceGroupId = deviceGroupId)
        expr

    let StoreSensorName (deviceGroupId : string) (propertyId : string) (propertyName : string) =
        
        let filter =
            GetSensorExpression deviceGroupId propertyId

        let update =
            Builders<StorableSensorState>.Update.Set((fun s -> s.PropertyName), propertyName)

        async {
            do! sensorNameSemaphore.WaitAsync() |> Async.AwaitTask

            try
                do! SensorsCollection.UpdateOneAsync<StorableSensorState>(filter, update)
                    |> Async.AwaitTask
                    |> Async.Ignore
                    
            finally
                sensorNameSemaphore.Release() |> ignore
        }

    let StoreSensorState (sensorState : StorableSensorState) =

        let filter = GetSensorExpression sensorState.DeviceGroupId sensorState.PropertyId
    
        let update =
            Builders<StorableSensorState>.Update             
             .Set((fun s -> s.DeviceGroupId), sensorState.DeviceGroupId)
             .Set((fun s -> s.DeviceId), sensorState.DeviceId)
             .Set((fun s -> s.PropertyId), sensorState.PropertyId)
             .Set((fun s -> s.PropertyName), sensorState.PropertyName)
             .Set((fun s -> s.PropertyType), sensorState.PropertyType)
             .Set((fun s -> s.PropertyValue), sensorState.PropertyValue)
             .Set((fun s -> s.BatteryVoltage), sensorState.BatteryVoltage)
             .Set((fun s -> s.SignalStrength), sensorState.SignalStrength)
             .Set((fun s -> s.LastUpdated), sensorState.LastUpdated)
             .Set((fun s -> s.LastActive), sensorState.LastActive)
    
        async {
            do! sensorStateSemaphore.WaitAsync() |> Async.AwaitTask

            try            
                do! SensorsCollection.UpdateOneAsync<StorableSensorState>(filter, update, BsonStorage.Upsert)
                    :> Tasks.Task
                    |> Async.AwaitTask

            finally
                sensorStateSemaphore.Release() |> ignore
        }

    let GetSensorState deviceGroupId propertyId : Async<StorableSensorState> =
        async {
            let filter = GetSensorExpression deviceGroupId propertyId

            let! sensorState =
                SensorsCollection.FindSync<StorableSensorState>(filter).SingleOrDefaultAsync()
                |> Async.AwaitTask

            return sensorState
        }

    let GetSensorStates deviceGroupId : Async<StorableSensorState list> =
        async {
            let filter = GetSensorsExpression deviceGroupId

            let! sensorStates =
                SensorsCollection.FindSync<StorableSensorState>(filter).ToListAsync()
                |> Async.AwaitTask

            return sensorStates |> List.ofSeq
        }

    let InitialState defaultName =
        { Id = ObjectId.Empty
          DeviceGroupId = ""
          GatewayId = ""
          DeviceId = ""
          PropertyId = ""
          PropertyName = defaultName
          PropertyDescription = defaultName
          PropertyType = ""
          PropertyValue = null
          Protocol = ""
          BatteryVoltage = 0.0
          SignalStrength = 0.0
          LastUpdated = DateTimeOffset.UtcNow
          LastActive = DateTimeOffset.UtcNow }
        
    let Drop() =
        BsonStorage.Database.DropCollection(SensorsCollectionName)
