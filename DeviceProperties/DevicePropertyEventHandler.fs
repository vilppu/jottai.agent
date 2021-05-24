namespace Jottai

module DevicePropertyEventHandler =

    open System
    open FSharp.Control.Reactive

    let handle publish (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.DevicePropertyChanged event ->
                let devicePropertyUpdate = event |> Event.ToDevicePropertyUpdate
                let! devicePropertyState = Action.GetDevicePropertyState devicePropertyUpdate

                do! Action.StoreDeviceProperty devicePropertyState
                publish (Event.DevicePropertyStored devicePropertyState)

            | Event.DevicePropertyNameChanged event ->                
                do! DevicePropertyStorage.StoreDevicePropertyName event.DeviceGroupId event.GatewayId event.DeviceId event.PropertyId event.PropertyName
                publish (Event.DevicePropertyNameStored event)
            | _ -> ()
        }
    
    let SubscribeTo publish (events : IObservable<Event.Event>) : IDisposable =
        let handle = handle publish
        events
        |> Observable.subscribe (fun event -> handle event |> Async.Start)
