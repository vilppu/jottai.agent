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
          mutable DeviceId : string
          mutable SensorId : string
          mutable SensorName : string
          mutable MeasuredProperty : string
          mutable MeasuredValue : obj
          mutable BatteryVoltage : float
          mutable SignalStrength : float
          mutable LastUpdated : DateTimeOffset
          mutable LastActive : DateTimeOffset }

    let private SensorsCollectionName = "Sensors"

    let private SensorsCollection = 
        BsonStorage.Database.GetCollection<StorableSensorState> SensorsCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"
    
    let private GetSensorExpression (deviceGroupId : string) (sensorId : string) =
        let sensorId = sensorId
        let deviceGroupId = deviceGroupId
        let expr = Expressions.Lambda.Create<StorableSensorState>(fun x -> x.DeviceGroupId = deviceGroupId && x.SensorId = sensorId)
        expr
    
    let private GetSensorsExpression (deviceGroupId : string) =        
        let deviceGroupId = deviceGroupId
        let expr = Expressions.Lambda.Create<StorableSensorState>(fun x -> x.DeviceGroupId = deviceGroupId)
        expr

    let StoreSensorName (deviceGroupId : string) (sensorId : string) (sensorName : string) =
        
        let filter =
            GetSensorExpression deviceGroupId sensorId

        let update =
            Builders<StorableSensorState>.Update.Set((fun s -> s.SensorName), sensorName)

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

        let filter = GetSensorExpression sensorState.DeviceGroupId sensorState.SensorId
    
        let update =
            Builders<StorableSensorState>.Update             
             .Set((fun s -> s.DeviceGroupId), sensorState.DeviceGroupId)
             .Set((fun s -> s.DeviceId), sensorState.DeviceId)
             .Set((fun s -> s.SensorId), sensorState.SensorId)
             .Set((fun s -> s.SensorName), sensorState.SensorName)
             .Set((fun s -> s.MeasuredProperty), sensorState.MeasuredProperty)
             .Set((fun s -> s.MeasuredValue), sensorState.MeasuredValue)
             .Set((fun s -> s.BatteryVoltage), sensorState.BatteryVoltage)
             .Set((fun s -> s.SignalStrength), sensorState.SignalStrength)
             .Set((fun s -> s.LastUpdated), sensorState.LastUpdated)
             .Set((fun s -> s.LastActive), sensorState.LastActive)
    
        async {
            do! sensorStateSemaphore.WaitAsync() |> Async.AwaitTask

            try            
                do! SensorsCollection.UpdateOneAsync<StorableSensorState>(filter, update, BsonStorage.Upsert)
                    :> System.Threading.Tasks.Task
                    |> Async.AwaitTask

            finally
                sensorStateSemaphore.Release() |> ignore
        }

    let GetSensorState deviceGroupId sensorId : Async<StorableSensorState> =
        async {
            let filter = GetSensorExpression deviceGroupId sensorId

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
          DeviceId = ""
          SensorId = ""
          SensorName = defaultName
          MeasuredProperty = ""
          MeasuredValue = null
          BatteryVoltage = 0.0
          SignalStrength = 0.0
          LastUpdated = DateTimeOffset.UtcNow
          LastActive = DateTimeOffset.UtcNow }
        
    let Drop() =
        BsonStorage.Database.DropCollection(SensorsCollectionName)
