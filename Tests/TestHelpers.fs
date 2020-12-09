namespace Jottai

[<AutoOpen>]
module TestHelpers =
    
    let SetupToReceivePushNotifications(context : Context) = 
        let result =
            SubscribeToPushNotifications context.DeviceGroupToken "12345"
            |> Async.RunSynchronously
        if not result.IsSuccessStatusCode then
            failwith "SubscribeToPushNotifications failed"
        |> ignore
    
    let WriteMeasurement (measurement, deviceId) (context : Context) = 
        PostMeasurement context.SensorToken deviceId measurement

    let WriteMeasurementSynchronously (measurement, deviceId) (context : Context) = 
        WriteMeasurement (measurement, deviceId) context |> Async.RunSynchronously |> ignore
    
    let GetExampleSensorStateResponse (context : Context) =
        context.WaitForNotification()
        GetSensorStateResponse context.DeviceGroupToken |> Async.RunSynchronously
    
    let GetExampleSensorState(context : Context) =
        context.WaitForNotification()
        GetSensorState context.DeviceGroupToken |> Async.RunSynchronously
    
    let GetExampleSensorHistoryResponse sensorId (context : Context) =
        context.WaitForNotification()
        GetSensorHistoryResponse context.DeviceGroupToken sensorId
        |> Async.RunSynchronously
    
    let GetExampleSensorHistory sensorId (context : Context) =
        context.WaitForNotification()
        GetSensorHistory context.DeviceGroupToken sensorId
        |> Async.RunSynchronously
