namespace Jottai

module SensorEventHandler =

    open System
    open FSharp.Control.Reactive

    let private Handle publish (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SensorStateChanged event ->
                let sensorStateUpdate = event |> Event.ToSensorStateUpdate
                let! sensorState = Action.GetSensorState sensorStateUpdate
                let! sensorHistory = Action.GetSensorHistory sensorStateUpdate
                do! Action.StoreSensorState sensorState
                do! Action.StoreSensorHistory sensorState sensorHistory
                publish (Event.SensorStateStored sensorState)

            | Event.SensorNameChanged event ->
                do! SensorStateStorage.StoreSensorName event.DeviceGroupId event.PropertyId event.PropertyName
                publish (Event.SensorNameStored event)

            | _ -> ()
        }
    
    let SubscribeTo publish (events : IObservable<Event.Event>) : IDisposable =
        let handle = Handle publish
        events
        |> Observable.subscribe (fun event -> handle event |> Async.Start)
