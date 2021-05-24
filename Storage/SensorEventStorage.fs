namespace Jottai

module SensorEventStorage =
    
    let private SensorEvents (deviceGroupId : DeviceGroupId) =
        let (DeviceGroupId deviceGroupId) = deviceGroupId
        let collectionName = "SensorEvents." + deviceGroupId
        BsonStorage.Database.GetCollection<SensorStateUpdate> collectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"
        |> BsonStorage.WithDescendingIndex "DeviceId"
        |> BsonStorage.WithDescendingIndex "Timestamp"
    
    let Drop deviceGroupId =
        let collection = SensorEvents deviceGroupId
        BsonStorage.Database.DropCollection(collection.CollectionNamespace.CollectionName)
    
    let StoreSensorEvent (sensorStateUpdate : SensorStateUpdate) =     
                                        
        async {
            let collection = SensorEvents sensorStateUpdate.DeviceGroupId
            do! collection.InsertOneAsync(sensorStateUpdate) |> Async.AwaitTask
        }
