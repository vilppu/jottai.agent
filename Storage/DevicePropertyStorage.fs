namespace Jottai

module DevicePropertyStorage =
    open System
    open System.Threading
    open MongoDB.Bson
    open MongoDB.Bson.Serialization.Attributes
    open MongoDB.Driver
    let sensorNameSemaphore = new SemaphoreSlim(1)
    let devicePropertySemaphore = new SemaphoreSlim(1)
    
    [<CLIMutable>]
    [<BsonIgnoreExtraElements>]
    type StorableDeviceProperty = 
        { [<BsonIgnoreIfDefault>]
          mutable Id : ObjectId
          mutable DeviceGroupId : string
          mutable GatewayId : string
          mutable DeviceId : string
          mutable PropertyId : string
          mutable PropertyType : string
          mutable PropertyName : string
          mutable PropertyDescription : string
          mutable PropertyValue : obj
          mutable Protocol : string
          mutable LastUpdated : DateTimeOffset
          mutable LastActive : DateTimeOffset }

    let private DevicePropertiesCollectionName = "DeviceProperties"

    let private DevicePropertiesCollection = 
        BsonStorage.Database.GetCollection<StorableDeviceProperty> DevicePropertiesCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"

    let private GetDevicePropertyExpression (deviceProperty : StorableDeviceProperty) =
        let deviceGroupId = deviceProperty.DeviceGroupId
        let gatewayId = deviceProperty.GatewayId
        let deviceId = deviceProperty.DeviceId
        let propertyId = deviceProperty.PropertyId
        
        let expr = Expressions.Lambda.Create<StorableDeviceProperty>(fun command ->
            command.DeviceGroupId = deviceGroupId &&
            command.GatewayId = gatewayId &&
            command.DeviceId = deviceId &&
            command.PropertyId = propertyId)
        expr

    let private GetDevicePropertiesExpression (deviceGroupId : string) =        
        let deviceGroupId = deviceGroupId
        let expr = Expressions.Lambda.Create<StorableDeviceProperty>(fun x -> x.DeviceGroupId = deviceGroupId)
        expr

    let StoreDeviceProperty (deviceProperty : StorableDeviceProperty) =

        let filter = GetDevicePropertyExpression deviceProperty
    
        let update =
            Builders<StorableDeviceProperty>.Update
             .Set((fun s -> s.DeviceGroupId), deviceProperty.DeviceGroupId)
             .Set((fun s -> s.GatewayId), deviceProperty.GatewayId)
             .Set((fun s -> s.DeviceId), deviceProperty.DeviceId)
             .Set((fun s -> s.PropertyId), deviceProperty.PropertyId)
             .Set((fun s -> s.PropertyType), deviceProperty.PropertyType)
             .Set((fun s -> s.PropertyName), deviceProperty.PropertyName)
             .Set((fun s -> s.PropertyDescription), deviceProperty.PropertyDescription)
             .Set((fun s -> s.PropertyValue), deviceProperty.PropertyValue)
             .Set((fun s -> s.Protocol), deviceProperty.Protocol)
             .Set((fun s -> s.LastUpdated), deviceProperty.LastUpdated)
             .Set((fun s -> s.LastActive), deviceProperty.LastActive)
    
        async {
            do! devicePropertySemaphore.WaitAsync() |> Async.AwaitTask

            try            
                do! DevicePropertiesCollection.UpdateOneAsync<StorableDeviceProperty>(filter, update, BsonStorage.Upsert)
                    :> Tasks.Task
                    |> Async.AwaitTask

            finally
                devicePropertySemaphore.Release() |> ignore
        }

    let GetDeviceProperties deviceGroupId : Async<StorableDeviceProperty list> =
        async {
            let filter = GetDevicePropertiesExpression deviceGroupId

            let! deviceProperties =
                DevicePropertiesCollection.FindSync<StorableDeviceProperty>(filter).ToListAsync()
                |> Async.AwaitTask

            return deviceProperties |> List.ofSeq
        }
        
    let Drop() =
        BsonStorage.Database.DropCollection(DevicePropertiesCollectionName)
