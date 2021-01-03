namespace Jottai

[<AutoOpen>]
module TestHelpers =
    open System
    open System.Threading

    let private waitTimeout = TimeSpan.FromSeconds(5.0)
    
    let WaitUntilSensorStateIsChanged asyncOperation =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.SensorStateStored _ -> waiter.Release() |> ignore
            | _ -> ()
            )
    
        asyncOperation |> Async.RunSynchronously |> ignore
        waiter.Wait(waitTimeout) |> ignore
    
    let WaitUntilPushNotificationsAreSent asyncOperation =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.PushNotificationsSent _ ->
                waiter.Release() |> ignore
            | _ -> ()
            )
    
        asyncOperation |> Async.RunSynchronously |> ignore
        waiter.Wait(waitTimeout) |> ignore
    
    let WaitUntilPushNotificationSubscriptionStored asyncOperation =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.PushNotificationSubscriptionStored _ -> waiter.Release() |> ignore
            | _ -> ()
            )
    
        asyncOperation |> Async.RunSynchronously |> ignore
        waiter.Wait(waitTimeout) |> ignore 
        
    let WaitUntilPollingDevicePropertyChangeRequests asyncOperation =
        async {
            use waiter = new SemaphoreSlim(0)
            use subscription = Application.Events.Subscribe(fun event ->
                match event with
                | Event.PollingDevicePropertyChangeRequests _ -> waiter.Release() |> ignore
                | _ -> ()
                )
        
            let! result = asyncOperation |> Async.StartChild
            waiter.Wait(waitTimeout) |> ignore
            return result
        }
    
    let WaitUntilDevicePropertyIsUpdated action =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.DevicePropertyStored _ -> waiter.Release() |> ignore
            | _ -> ()
            )
    
        action |> Async.RunSynchronously |> ignore
        waiter.Wait(waitTimeout) |> ignore
    
    let WaitUntilDevicePropertyNameIsChanged action =
        use waiter = new SemaphoreSlim(0)
        use subscription = Application.Events.Subscribe(fun event ->
            match event with
            | Event.DevicePropertyNameStored _ -> waiter.Release() |> ignore
            | _ -> ()
            )
    
        action |> Async.RunSynchronously |> ignore
        waiter.Wait(waitTimeout) |> ignore

    let SetupToReceivePushNotifications(context : Context) = 
        let result =
            SubscribeToPushNotifications context.DeviceGroupToken "12345"
            |> Async.RunSynchronously
        if not result.IsSuccessStatusCode then
            failwith "SubscribeToPushNotifications failed"
        |> ignore
    
    let WriteMeasurement (measurement, deviceId) (context : Context) =
        PostMeasurement context.DeviceToken deviceId measurement

    let WriteMeasurementSynchronously (measurement, deviceId) (context : Context) : unit =
        WriteMeasurement (measurement, deviceId) context
        |> WaitUntilSensorStateIsChanged
        
    let WriteDeviceDataSynchronously deviceData (context : Context) : unit =        
        PostDeviceData (context.DeviceToken) deviceData
        |> WaitUntilSensorStateIsChanged
    
    let SensorStateResponse (context : Context) =
        GetSensorStateResponse context.DeviceGroupToken |> Async.RunSynchronously
    
    let SensorState(context : Context) =
        GetSensorState context.DeviceGroupToken |> Async.RunSynchronously
    
    let SensorHistoryResponse propertyId (context : Context) =
        GetSensorHistoryResponse context.DeviceGroupToken propertyId
        |> Async.RunSynchronously
    
    let SensorHistory propertyId (context : Context) =
        GetSensorHistory context.DeviceGroupToken propertyId
        |> Async.RunSynchronously        
        
    let DeviceProperties(context : Context) =
        GetDeviceProperties context.DeviceGroupToken |> Async.RunSynchronously