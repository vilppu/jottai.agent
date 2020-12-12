namespace Jottai

[<AutoOpen>]
module TestHelpers =
    open System
    open System.Threading

    let SetupToReceivePushNotifications(context : Context) = 
        let result =
            SubscribeToPushNotifications context.DeviceGroupToken "12345"
            |> Async.RunSynchronously
        if not result.IsSuccessStatusCode then
            failwith "SubscribeToPushNotifications failed"
        |> ignore
    
    let WriteMeasurement (measurement, deviceId) (context : Context) =
        PostMeasurement context.SensorToken deviceId measurement

    let WriteMeasurementSynchronously (measurement, deviceId) (context : Context) : unit =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.Event.SensorStateChangeCompleted _ -> waiter.Release() |> ignore
            | _ -> ()
            )

        WriteMeasurement (measurement, deviceId) context |> Async.RunSynchronously |> ignore
        waiter.Wait(TimeSpan.FromSeconds(10.0)) |> ignore
        
    let WriteDeviceDataSynchronously deviceData (context : Context) : unit =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.Event.SensorStateChangeCompleted _ -> waiter.Release() |> ignore
            | _ -> ()
            )
        
        PostSensorData (context.SensorToken) deviceData |> Async.RunSynchronously |> ignore
        waiter.Wait(TimeSpan.FromSeconds(10.0)) |> ignore
    
    let GetExampleSensorStateResponse (context : Context) =
        GetSensorStateResponse context.DeviceGroupToken |> Async.RunSynchronously
    
    let GetExampleSensorState(context : Context) =
        GetSensorState context.DeviceGroupToken |> Async.RunSynchronously
    
    let GetExampleSensorHistoryResponse sensorId (context : Context) =
        GetSensorHistoryResponse context.DeviceGroupToken sensorId
        |> Async.RunSynchronously
    
    let GetExampleSensorHistory sensorId (context : Context) =
        GetSensorHistory context.DeviceGroupToken sensorId
        |> Async.RunSynchronously
