namespace Jottai

module SensorHistoryStorage =
    open System
    open System.Collections.Generic
    open MongoDB.Bson
    open MongoDB.Bson.Serialization.Attributes
    open MongoDB.Driver
    open Jottai.Expressions

    [<CLIMutable>]
    [<BsonIgnoreExtraElements>]
    type StorableSensorHistoryEntry = 
        { [<BsonIgnoreIfDefault>]
          Id : ObjectId
          MeasuredValue : obj
          Timestamp : DateTimeOffset }

    [<CLIMutable>]
    [<BsonIgnoreExtraElements>]
    type StorableSensorHistory = 
        { 
          [<BsonIgnoreIfDefault>]
          Id : ObjectId
          DeviceGroupId : string
          SensorId : string
          MeasuredProperty : string
          Entries : List<StorableSensorHistoryEntry> }

    let private SensorHistoryCollectionName = "SensorHistory"

    let private SensorHistoryCollection = 
        BsonStorage.Database.GetCollection<StorableSensorHistory> SensorHistoryCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"        
        |> BsonStorage.WithDescendingIndex "DeviceId"
        |> BsonStorage.WithDescendingIndex "MeasuredProperty"
    
    let private FilterHistoryBy (deviceGroupId : string) (sensorId : string) =
        let sensorId = sensorId
        let deviceGroupId = deviceGroupId
        let expr = Lambda.Create<StorableSensorHistory>(fun x -> x.DeviceGroupId = deviceGroupId && x.SensorId = sensorId)
        expr

    let GetSensorHistory (deviceGroupId : string) (sensorId : string)
        : Async<StorableSensorHistory> =
        async {
            let filter = FilterHistoryBy deviceGroupId sensorId
            let! history =
                SensorHistoryCollection.Find<StorableSensorHistory>(filter).FirstOrDefaultAsync<StorableSensorHistory>()
                |> Async.AwaitTask
            return history
        }
        
    let UpsertSensorHistory (history : StorableSensorHistory) =       
          
        let filter = FilterHistoryBy history.DeviceGroupId history.SensorId
        
        SensorHistoryCollection.ReplaceOneAsync<StorableSensorHistory>(filter, history, BsonStorage.Replace)
        |> Async.AwaitTask
        |> Async.Ignore
    
    let Drop() = BsonStorage.Database.DropCollection(SensorHistoryCollectionName)