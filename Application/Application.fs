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

    let PostSensorName deviceGroupId sensorId sensorName : Async<unit> = 
        async {    
            let changeSensorName : Command.ChangeSensorName =
                { SensorId = SensorId sensorId
                  DeviceGroupId = DeviceGroupId deviceGroupId
                  SensorName = sensorName}
            let command = Command.ChangeSensorName changeSensorName
            do! Command.Execute command
        }    
    
    let GetSensorStates (deviceGroupId : string) : Async<ApiObjects.SensorState list> = 
        async {        
            let! statuses = SensorStateStorage.GetSensorStates deviceGroupId

            return
                statuses
                |> ConvertSensorState.FromStorables
                |> ConvertSensorState.ToApiObjects
        }

    let GetSensorHistory (deviceGroupId : string) (sensorId : string) : Async<ApiObjects.SensorHistory> =
        async {
            let! history = SensorHistoryStorage.GetSensorHistory deviceGroupId sensorId
            let result =
                history
                |> ConvertSensorHistory.FromStorable
                |> ConvertSensorHistory.ToApiObject
            return result
        }   
   
    let GetDeviceProperties (deviceGroupId : string) : Async<ApiObjects.DevicePropertyState list> =
        async {
            let! commands = DevicePropertyStorage.GetDeviceProperties deviceGroupId
            let result =
                commands
                |> ConvertDeviceProperty.FromStorables
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
            match Command.FromDeviceProperty deviceGroupId gatewayId deviceId propertyId propertyType propertyValue with
            | Some command -> do! Command.Execute command
            | _ -> ()
        }

    let GetDevicePropertyChangeRequest (deviceGroupId : string) : Async<ApiObjects.DevicePropertyChangeRequest option> =
        async {
            let deviceGroupId = DeviceGroupId deviceGroupId

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
