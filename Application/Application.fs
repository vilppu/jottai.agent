namespace Jottai

module Application =
    open System
    open FSharp.Control.Reactive
        
    let HasTokenSecret =
        let tokenSecret = Environment.GetEnvironmentVariable("JOTTAI_TOKEN_SECRET")
        if tokenSecret |> isNull then
            false
        else
            true    
    
    let Authority() : string =
        let authority = Environment.GetEnvironmentVariable("JOTTAI_AUTHORITY")
        if authority |> isNull then
            eprintfn "Environment variable JOTTAI_AUTHORITY is not set."
            String.Empty
        else
            authority
            
    let Audience() : string =
        let audience = Environment.GetEnvironmentVariable("JOTTAI_AUDIENCE")
        if audience |> isNull then
            eprintfn "Environment variable JOTTAI_AUDIENCE is not set."
            String.Empty
        else
            audience
            
    let ClientId() : string =
        let clientId = Environment.GetEnvironmentVariable("JOTTAI_CLIENT_ID")
        if clientId |> isNull then
            eprintfn "Environment variable JOTTAI_CLIENT_ID is not set."
            String.Empty
        else
            clientId
    
    let ManagementClientId() : string =
        let authority = Environment.GetEnvironmentVariable("JOTTAI_MANAGEMENT_CLIENT_ID")
        if authority |> isNull then
            eprintfn "Environment variable JOTTAI_MANAGEMENT_CLIENT_ID is not set."
            String.Empty
        else
            authority
    
    let ManagementClientSecret() : string =
        let authority = Environment.GetEnvironmentVariable("JOTTAI_MANAGEMENT_CLIENT_SECRET")
        if authority |> isNull then
            eprintfn "Environment variable JOTTAI_MANAGEMENT_CLIENT_SECRET is not set."
            String.Empty
        else
            authority
    
    let ManagementAudience() : string =
        let authority = Environment.GetEnvironmentVariable("JOTTAI_MANAGEMENT_AUDIENCE")
        if authority |> isNull then
            eprintfn "Environment variable JOTTAI_MANAGEMENT_AUDIENCE is not set."
            String.Empty
        else
            authority

    let GenerateDeviceGroupId() : string =
        let tokenBytes = Array.zeroCreate<byte> 16
        System.Security.Cryptography.RandomNumberGenerator.Create().GetBytes tokenBytes
        let tokenWithDashes = BitConverter.ToString tokenBytes
        let token = tokenWithDashes.Replace("-", "").ToLower()
        token

    let PostSensorName deviceGroupId propertyId propertyName : Async<unit> = 
        async {
            let propertyName = ValidatePropertyName propertyName
            match propertyName with
            | Some propertyName -> 
                let changeSensorName : Command.ChangeSensorName =
                    { PropertyId = PropertyId propertyId
                      DeviceGroupId = DeviceGroupId deviceGroupId
                      PropertyName = propertyName}
                let command = Command.ChangeSensorName changeSensorName
                do! Command.Execute command
            | None -> ()
        }

    let GetSensorStates (deviceGroupId : string) : Async<ApiObjects.SensorState list> = 
        async {        
            let! statuses = SensorStateStorage.GetSensorStates (DeviceGroupId deviceGroupId)

            return
                statuses
                |> ConvertSensorState.ToApiObjects
        }

    let GetSensorHistory (deviceGroupId : DeviceGroupId) (propertyId : PropertyId) : Async<ApiObjects.SensorHistory> =
        async {
            let! history = SensorHistoryStorage.GetSensorHistory deviceGroupId propertyId
            let result =
                history
                |> ConvertSensorHistory.ToApiObject
            return result
        }   
   
    let GetDeviceProperties (deviceGroupId : DeviceGroupId) : Async<ApiObjects.DeviceProperty list> =
        async {
            let! commands = DevicePropertyStorage.GetDeviceProperties deviceGroupId
            let result =
                commands
                |> ConvertDeviceProperty.ToApiObjects
            return result
        }

    let SubscribeToPushNotifications deviceGroupId (token : string) : Async<unit> = 
        async {
            let subscription = Notification.Subscription token
            let subscribeToPushNotifications : Command.SubscribeToPushNotifications =
                { DeviceGroupId = (DeviceGroupId deviceGroupId)
                  Subscription = subscription }
            let command = Command.SubscribeToPushNotifications subscribeToPushNotifications
            do! Command.Execute command
        }

    let PostDeviceData deviceGroupId (deviceData : ApiObjects.DeviceData) =
        async {
            for command in Commands.FromDeviceData deviceGroupId deviceData do                
                do! Command.Execute command
        }

    let PostDevicePropertyValue
        (deviceGroupId : string)
        (gatewayId : string)
        (deviceId : string)
        (propertyId : string)
        (propertyType : string)
        (propertyValue : string)
        : Async<unit> =
        async {
            match Command.FromDevicePropertyValue deviceGroupId gatewayId deviceId propertyId propertyType propertyValue with
            | Some command -> do! Command.Execute command
            | _ -> ()
        }

    let PostDevicePropertyName
        (deviceGroupId : string)
        (gatewayId : string)
        (deviceId : string)
        (propertyId : string)
        (propertyType : string)
        (propertyName : string)
        : Async<unit> =
        async {
            match Command.FromDevicePropertyName deviceGroupId gatewayId deviceId propertyId propertyType propertyName with
            | Some command -> do! Command.Execute command
            | _ -> ()
        }

    let GetDevicePropertyChangeRequest (deviceGroupId : string) : Async<ApiObjects.DevicePropertyChangeRequest option> =
        async {
            let deviceGroupId = DeviceGroupId deviceGroupId

            EventBus.Publish Event.PollingDevicePropertyChangeRequests

            let! devicePropertyChangeRequest =
                WaitForDevicePropertyChangeRequestedEvent.For deviceGroupId

            let result =
                match devicePropertyChangeRequest with
                | Some devicePropertyChangeRequest ->
                    devicePropertyChangeRequest
                    |> ConvertDevicePropertyChangeRequest.ToApiObject
                    |> Some
                | None -> None 

            return result
        }
    
    let StartProcessingEvents httpSend : IDisposable =
        let sensorEventSubscription = SensorEventHandler.SubscribeTo EventBus.Publish
        let devicePropertyEventSubscription = DevicePropertyEventHandler.SubscribeTo EventBus.Publish
        let pushNotificationEventSubscription = PushNotificationEventHandler.SubscribeTo httpSend EventBus.Publish

        EventBus.Disposable
        |> Disposable.compose (sensorEventSubscription EventBus.Events)
        |> Disposable.compose (devicePropertyEventSubscription EventBus.Events)
        |> Disposable.compose (pushNotificationEventSubscription EventBus.Events)

    let Events =
        EventBus.Events
