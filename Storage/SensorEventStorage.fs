namespace Jottai

module SensorEventStorage =
    open System
    open MongoDB.Bson
    open MongoDB.Bson.Serialization.Attributes

    [<CLIMutable>]
    type StorableSensorEvent = 
        { [<BsonIgnoreIfDefault>]
          Id : ObjectId
          DeviceGroupId : string
          DeviceId : string
          PropertyId : string
          PropertyType : string
          PropertyValue : obj
          BatteryVoltage : float
          SignalStrength : float
          Timestamp : DateTimeOffset }

    let private StorableSensorEvent (sensorStateUpdate : SensorStateUpdate)
        : StorableSensorEvent =
        let (DeviceGroupId deviceGroupId) = sensorStateUpdate.DeviceGroupId
        let (DeviceId deviceId) = sensorStateUpdate.DeviceId
        let (PropertyId propertyId) = sensorStateUpdate.PropertyId
        let propertyType = sensorStateUpdate.Measurement |> Measurement.Name
        let propertyValue = sensorStateUpdate.Measurement |> Measurement.Value
        let batteryVoltage = float(sensorStateUpdate.BatteryVoltage)
        let signalStrength = sensorStateUpdate.SignalStrength
        let timestamp = sensorStateUpdate.Timestamp

        { Id = new ObjectId()
          DeviceGroupId = deviceGroupId
          DeviceId = deviceId
          PropertyId = propertyId
          PropertyType = propertyType
          PropertyValue = propertyValue
          BatteryVoltage = batteryVoltage
          SignalStrength = signalStrength
          Timestamp = timestamp }        
    
    let private SensorEvents (deviceGroupId : DeviceGroupId) =
        let (DeviceGroupId deviceGroupId) = deviceGroupId
        let collectionName = "SensorEvents." + deviceGroupId
        BsonStorage.Database.GetCollection<StorableSensorEvent> collectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"
        |> BsonStorage.WithDescendingIndex "DeviceId"
        |> BsonStorage.WithDescendingIndex "Timestamp"
    
    let Drop deviceGroupId =
        let collection = SensorEvents deviceGroupId
        BsonStorage.Database.DropCollection(collection.CollectionNamespace.CollectionName)
    
    let StoreSensorEvent (sensorStateUpdate : SensorStateUpdate) =
                                        
        async {
            let collection = SensorEvents sensorStateUpdate.DeviceGroupId
            let storable = sensorStateUpdate |> StorableSensorEvent
            do! collection.InsertOneAsync(storable) |> Async.AwaitTask
        }
