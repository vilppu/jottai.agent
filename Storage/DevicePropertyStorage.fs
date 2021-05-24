namespace Jottai

module DevicePropertyStorage =
    open System.Threading
    open MongoDB.Driver
    let sensorNameSemaphore = new SemaphoreSlim(1)
    let devicePropertySemaphore = new SemaphoreSlim(1)

    let private DevicePropertiesCollectionName = "DeviceProperties"

    let private DevicePropertiesCollection = 
        BsonStorage.Database.GetCollection<DevicePropertyState> DevicePropertiesCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"

    let private GetDevicePropertyExpression deviceGroupId gatewayId deviceId propertyId =
        
        let expr = Expressions.Lambda.Create<DevicePropertyState>(fun command ->
            command.DeviceGroupId = deviceGroupId &&
            command.GatewayId = gatewayId &&
            command.DeviceId = deviceId &&
            command.PropertyId = propertyId)
        expr        

    let private GetDevicePropertiesExpression (deviceGroupId : DeviceGroupId) =
        let expr = Expressions.Lambda.Create<DevicePropertyState>(fun x -> x.DeviceGroupId = deviceGroupId)
        expr
        
    let GetDeviceProperty (update : DevicePropertyStateUpdate) : Async<DevicePropertyState> =

        async {
            let filter = GetDevicePropertyExpression update.DeviceGroupId update.GatewayId update.DeviceId update.PropertyId
            
        
            let! deviceProperties =
                DevicePropertiesCollection.FindSync<DevicePropertyState>(filter).ToListAsync()
                |> Async.AwaitTask
                    
            let previousState =
                if deviceProperties.Count = 1
                then Some (deviceProperties.[0])
                else None
                
            let lastActive = update.Timestamp
            let lastUpdated =
                match previousState with
                | Some previousState ->
                    if update.PropertyValue <> previousState.PropertyValue
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
    
        let update =
            Builders<DevicePropertyState>.Update
             .Set((fun s -> s.PropertyName), propertyName)
    
        async {
            do! devicePropertySemaphore.WaitAsync() |> Async.AwaitTask

            try            
                do! DevicePropertiesCollection.UpdateOneAsync<DevicePropertyState>(filter, update, BsonStorage.Upsert)
                    :> Tasks.Task
                    |> Async.AwaitTask

            finally
                devicePropertySemaphore.Release() |> ignore
        }

    let StoreDeviceProperty (deviceProperty : DevicePropertyState) =        
            async {
            
            let filter = GetDevicePropertyExpression deviceProperty.DeviceGroupId deviceProperty.GatewayId deviceProperty.DeviceId deviceProperty.PropertyId       
    
            let update =
                Builders<DevicePropertyState>.Update
                 .Set((fun s -> s.DeviceGroupId), deviceProperty.DeviceGroupId)
                 .Set((fun s -> s.GatewayId), deviceProperty.GatewayId)
                 .Set((fun s -> s.DeviceId), deviceProperty.DeviceId)
                 .Set((fun s -> s.PropertyId), deviceProperty.PropertyId)
                 .Set((fun s -> s.PropertyName), deviceProperty.PropertyName)
                 .Set((fun s -> s.PropertyDescription), deviceProperty.PropertyDescription)
                 .Set((fun s -> s.PropertyValue), deviceProperty.PropertyValue)
                 .Set((fun s -> s.Protocol), deviceProperty.Protocol)
                 .Set((fun s -> s.LastUpdated), deviceProperty.LastUpdated)
                 .Set((fun s -> s.LastActive), deviceProperty.LastActive)

            do! devicePropertySemaphore.WaitAsync() |> Async.AwaitTask

            try            
                do! DevicePropertiesCollection.UpdateOneAsync<DevicePropertyState>(filter, update, BsonStorage.Upsert)
                    :> Tasks.Task
                    |> Async.AwaitTask

            finally
                devicePropertySemaphore.Release() |> ignore
        }

    let GetDeviceProperties deviceGroupId : Async<DevicePropertyState list> =
        async {
            let filter = GetDevicePropertiesExpression deviceGroupId

            let! deviceProperties =
                DevicePropertiesCollection.FindSync<DevicePropertyState>(filter).ToListAsync()
                |> Async.AwaitTask

            return deviceProperties |> List.ofSeq
        }
        
    let Drop() =
        BsonStorage.Database.DropCollection(DevicePropertiesCollectionName)
