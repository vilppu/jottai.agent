namespace Jottai

module DevicePropertyEventHandler =

    open System
    open FSharp.Control.Reactive

    let handle publish (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.DevicePropertyAvailable deviceProperty ->
                do! Action.StoreDeviceProperty deviceProperty
                publish (Event.DevicePropertyStored deviceProperty)
            | _ -> ()
        }
    
    let SubscribeTo publish (events : IObservable<Event.Event>) : IDisposable =
        let handle = handle publish
        events
        |> Observable.subscribe (fun event -> handle event |> Async.Start)
