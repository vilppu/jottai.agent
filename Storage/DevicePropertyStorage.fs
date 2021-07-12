namespace Jottai

module DevicePropertyStorage =
    open System
    open System.Threading
    open MongoDB.Bson
    open MongoDB.Bson.Serialization.Attributes
    open MongoDB.Driver
    
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

    let private StorableDeviceProperty (devicePropertyState : DevicePropertyState)
        : StorableDeviceProperty =
        let (DeviceGroupId deviceGroupId) = devicePropertyState.DeviceGroupId
        let (GatewayId gatewayId) = devicePropertyState.GatewayId
        let (DeviceId deviceId) = devicePropertyState.DeviceId
        let (PropertyId propertyId) = devicePropertyState.PropertyId
        let (PropertyName propertyName) = devicePropertyState.PropertyName
        let (PropertyDescription propertyDescription) = devicePropertyState.PropertyDescription
        let propertyType = devicePropertyState.PropertyValue |> DeviceProperty.Name
        let propertyValue = devicePropertyState.PropertyValue |> DeviceProperty.Value
        let protocol = 
            match devicePropertyState.Protocol with
               | ZWave -> "ZWave"
               | ZWavePlus -> "ZWavePlus"
               | _ -> "NotSpecified"
        let lastUpdated = devicePropertyState.LastUpdated
        let lastActive = devicePropertyState.LastActive     

        { Id = new ObjectId()
          DeviceGroupId = deviceGroupId
          GatewayId = gatewayId
          DeviceId = deviceId
          PropertyId = propertyId
          PropertyType = propertyType
          PropertyName = propertyName
          PropertyDescription = propertyDescription
          PropertyValue = propertyValue
          Protocol = protocol
          LastUpdated = lastUpdated
          LastActive = lastActive}

    let private DevicePropertyState (storableDeviceProperty : StorableDeviceProperty)
        : DevicePropertyState =
        
        let protocol = 
            match storableDeviceProperty.Protocol with
               | "ZWave" -> ZWave
               | "ZWavePlus" -> ZWavePlus
               | _ -> NotSpecified

        { DeviceGroupId = DeviceGroupId storableDeviceProperty.DeviceGroupId        
          GatewayId = GatewayId storableDeviceProperty.GatewayId
          DeviceId = DeviceId storableDeviceProperty.DeviceId
          PropertyId = PropertyId storableDeviceProperty.PropertyId
          PropertyName = PropertyName storableDeviceProperty.PropertyName          
          PropertyDescription = PropertyDescription storableDeviceProperty.PropertyDescription
          PropertyValue = (DeviceProperty.From storableDeviceProperty.PropertyType storableDeviceProperty.PropertyValue).Value
          Protocol = protocol          
          LastUpdated = storableDeviceProperty.LastUpdated
          LastActive = storableDeviceProperty.LastActive }

    let sensorNameSemaphore = new SemaphoreSlim(1)
    let devicePropertySemaphore = new SemaphoreSlim(1)

    let private DevicePropertiesCollectionName = "DeviceProperties"

    let private DevicePropertiesCollection = 
        BsonStorage.Database.GetCollection<StorableDeviceProperty> DevicePropertiesCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"

    let private GetDevicePropertyExpression deviceGroupId gatewayId deviceId propertyId =
        let (DeviceGroupId deviceGroupId) = deviceGroupId
        let (GatewayId gatewayId) = gatewayId
        let (DeviceId deviceId) = deviceId
        let (PropertyId propertyId) = propertyId
        
        let expr = Expressions.Lambda.Create<StorableDeviceProperty>(fun command ->
            command.DeviceGroupId = deviceGroupId &&
            command.GatewayId = gatewayId &&
            command.DeviceId = deviceId &&
            command.PropertyId = propertyId)
        expr        

    let private GetDevicePropertiesExpression (deviceGroupId : DeviceGroupId) =
        let (DeviceGroupId deviceGroupId) = deviceGroupId
        let expr = Expressions.Lambda.Create<StorableDeviceProperty>(fun x -> x.DeviceGroupId = deviceGroupId)
        expr
        
    let GetDeviceProperty (update : DevicePropertyStateUpdate) : Async<DevicePropertyState> =

        async {
            let filter = GetDevicePropertyExpression update.DeviceGroupId update.GatewayId update.DeviceId update.PropertyId
            
        
            let! deviceProperties =
                DevicePropertiesCollection.FindSync<StorableDeviceProperty>(filter).ToListAsync()
                |> Async.AwaitTask
                    
            let previousState =
                if deviceProperties.Count = 1
                then Some (deviceProperties.[0])
                else None               
            
            let lastActive = update.Timestamp
            let lastUpdated =
                match previousState with
                | Some previousState ->
                    let previousPropertyValue = (DeviceProperty.From previousState.PropertyType previousState.PropertyValue).Value
                    if update.PropertyValue <> previousPropertyValue
                    then update.Timestamp
                    else previousState.LastUpdated
                | None -> update.Timestamp
            
            return
              { DeviceGroupId = update.DeviceGroupId
                GatewayId = update.GatewayId
                DeviceId = update.DeviceId
                PropertyId = update.PropertyId
                PropertyName = update.PropertyName
                PropertyDescription = update.PropertyDescription
                PropertyValue = update.PropertyValue
                Protocol = update.Protocol
                LastUpdated = lastUpdated 
                LastActive = lastActive }            
         }

    let StoreDevicePropertyName deviceGroupId gatewayId deviceId propertyId propertyName =

        let filter = GetDevicePropertyExpression deviceGroupId gatewayId deviceId propertyId
        let (PropertyName propertyName) = propertyName
        let update =
            Builders<StorableDeviceProperty>.Update
             .Set((fun s -> s.PropertyName), propertyName)
    
        async {
            do! devicePropertySemaphore.WaitAsync() |> Async.AwaitTask

            try            
                do! DevicePropertiesCollection.UpdateOneAsync<StorableDeviceProperty>(filter, update, BsonStorage.Upsert)
                    :> Tasks.Task
                    |> Async.AwaitTask

            finally
                devicePropertySemaphore.Release() |> ignore
        }

    let StoreDeviceProperty (deviceProperty : DevicePropertyState) =        
            async {
            
            let filter = GetDevicePropertyExpression deviceProperty.DeviceGroupId deviceProperty.GatewayId deviceProperty.DeviceId deviceProperty.PropertyId           
            let storable = deviceProperty |> StorableDeviceProperty

            do! devicePropertySemaphore.WaitAsync() |> Async.AwaitTask

            try            
                do! DevicePropertiesCollection.ReplaceOneAsync<StorableDeviceProperty>(filter, storable, BsonStorage.Replace)
                    :> Tasks.Task
                    |> Async.AwaitTask

            finally
                devicePropertySemaphore.Release() |> ignore
        }

    let GetDeviceProperties deviceGroupId : Async<DevicePropertyState list> =
        async {
            let filter = GetDevicePropertiesExpression deviceGroupId

            let! deviceProperties =
                DevicePropertiesCollection.FindSync<StorableDeviceProperty>(filter).ToListAsync()
                |> Async.AwaitTask

            return deviceProperties |> Seq.map DevicePropertyState |> List.ofSeq
        }
        
    let Drop() =
        BsonStorage.Database.DropCollection(DevicePropertiesCollectionName)
