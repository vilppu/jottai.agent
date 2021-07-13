namespace Jottai

module WaitForDevicePropertyChangeRequestedEvent =
    open System
    open System.Threading

    let private timeout = TimeSpan.FromSeconds(60.0)

    let For (deviceGroupId: DeviceGroupId)  =
        async {
            use waiter = new SemaphoreSlim(0)
            let mutable result : Option<Event.DevicePropertyChangeRequest> = None

            use _ =
                EventBus.Subscribe
                    (fun event ->
                        async {
                            match event with
                            | Event.DevicePropertyChangeRequested event ->
                                if event.DeviceGroupId = deviceGroupId then
                                    waiter.Release() |> ignore
                                    result <- Some event
                            | _ -> ()
                        })

            do!
                waiter.WaitAsync(timeout)
                |> Async.AwaitTask
                |> Async.Ignore

            return result
        }
