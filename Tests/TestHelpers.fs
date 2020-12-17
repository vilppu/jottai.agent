namespace Jottai

[<AutoOpen>]
module TestHelpers =
    open System
    open System.Threading
    
    let WaitUntilSensorStateIsChanged asyncOperation =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.SensorStateStored _ -> waiter.Release() |> ignore
            | _ -> ()
            )
    
        asyncOperation |> Async.RunSynchronously |> ignore
        waiter.Wait(TimeSpan.FromSeconds(1.0)) |> ignore
    
    let WaitUntilPushNotificationsAreSent asyncOperation =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.PushNotificationsSent _ -> waiter.Release() |> ignore
            | _ -> ()
            )
    
        asyncOperation |> Async.RunSynchronously |> ignore
        waiter.Wait(TimeSpan.FromSeconds(1.0)) |> ignore
    
    let WaitUntilPushNotificationSubscriptionStored asyncOperation =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.PushNotificationSubscriptionStored _ -> waiter.Release() |> ignore
            | _ -> ()
            )
    
        asyncOperation |> Async.RunSynchronously |> ignore
        waiter.Wait(TimeSpan.FromSeconds(1.0)) |> ignore
    
    let WaitUntilDevicePropertyIsUpdated action =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.DevicePropertyStored _ -> waiter.Release() |> ignore
            | _ -> ()
            )
    
        action |> Async.RunSynchronously |> ignore
        waiter.Wait(TimeSpan.FromSeconds(1.0)) |> ignore

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
        WriteMeasurement (measurement, deviceId) context
        |> WaitUntilSensorStateIsChanged
        
    let WriteDeviceDataSynchronously deviceData (context : Context) : unit =        
        PostDevicData (context.SensorToken) deviceData
        |> WaitUntilSensorStateIsChanged
    
    let SensorStateResponse (context : Context) =
        GetSensorStateResponse context.DeviceGroupToken |> Async.RunSynchronously
    
    let SensorState(context : Context) =
        GetSensorState context.DeviceGroupToken |> Async.RunSynchronously
    
    let SensorHistoryResponse sensorId (context : Context) =
        GetSensorHistoryResponse context.DeviceGroupToken sensorId
        |> Async.RunSynchronously
    
    let SensorHistory sensorId (context : Context) =
        GetSensorHistory context.DeviceGroupToken sensorId
        |> Async.RunSynchronously        
        
    let DeviceProperties(context : Context) =
        GetDeviceProperties context.DeviceGroupToken |> Async.RunSynchronously