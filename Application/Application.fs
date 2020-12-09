namespace Jottai

module Application =
    open System
    open ApiObjects
    open System.Security.Cryptography

    let GenerateSecureToken() =         
        let tokenBytes = Array.zeroCreate<byte> 16
        RandomNumberGenerator.Create().GetBytes tokenBytes
        let tokenWithDashes = BitConverter.ToString tokenBytes
        tokenWithDashes.Replace("-", "")
        
    let HasTokenSecret =
        let tokenSecret = Environment.GetEnvironmentVariable("JOTTAI_TOKEN_SECRET")
        if tokenSecret |> isNull then
            false
        else
            true

    let TokenSecret() : string =
        let tokenSecret = Environment.GetEnvironmentVariable("JOTTAI_TOKEN_SECRET")
        if tokenSecret |> isNull then
            eprintfn "Environment variable JOTTAI_TOKEN_SECRET is not set."
            String.Empty
        else
            tokenSecret
        
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

    let PostSensorName httpSend deviceGroupId sensorId sensorName : Async<unit> = 
        async {    
            let changeSensorName : Command.ChangeSensorName =
                { SensorId = SensorId sensorId
                  DeviceGroupId = DeviceGroupId deviceGroupId
                  SensorName = sensorName}
            let command = Command.ChangeSensorName changeSensorName
            do! Command.Execute httpSend command
        }    
    
    let GetSensorState (deviceGroupId : string) : Async<ApiObjects.SensorState list> = 
        async {
        
            let! statuses = SensorStateStorage.GetSensorStates deviceGroupId

            return
                statuses
                |> FromStorables
                |> ToApiObjects
        }

    let GetSensorHistory (deviceGroupId : string) (sensorId : string) : Async<ApiObjects.SensorHistory> =
        async {
            let! history = SensorHistoryStorage.GetSensorHistory deviceGroupId sensorId
            let result =
                history
                |> FromStorable
                |> ToApiObject
            return result
        }
    
    let SubscribeToPushNotifications httpSend deviceGroupId (token : string) : Async<unit> = 
        async {
            let subscription = Notification.Subscription token
            let subscribeToPushNotifications : Command.SubscribeToPushNotifications =
                { DeviceGroupId = (DeviceGroupId deviceGroupId)
                  Subscription = subscription }
            let command = Command.SubscribeToPushNotifications subscribeToPushNotifications
            do! Command.Execute httpSend command
        }

    let PostDeviceData httpSend deviceGroupId (deviceData : DeviceData) =
        async {
            let sensorStateUpdates = deviceData |> ToSensorStateUpdates (DeviceGroupId deviceGroupId)
            let changeSensorStates = Command.From sensorStateUpdates
            for changeSensorState in changeSensorStates do                
                do! Command.Execute httpSend changeSensorState
        }
