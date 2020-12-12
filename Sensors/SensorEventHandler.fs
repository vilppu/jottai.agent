namespace Jottai

module SensorEventHandler =

    open System
    open FSharp.Control.Reactive

    let handle publish (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SensorStateChanged event ->
                let sensorStateUpdate = event |> Event.ToSensorStateUpdate
                let! sensorState = Action.GetSensorState sensorStateUpdate
                let! sensorHistory = Action.GetSensorHistory sensorStateUpdate               
                do! Action.StoreSensorState sensorState
                do! Action.StoreSensorHistory sensorState sensorHistory
                publish (Event.SensorStateChangeCompleted sensorState)

            | Event.SensorNameChanged event ->
                do! SensorStateStorage.StoreSensorName event.DeviceGroupId.AsString event.SensorId.AsString event.SensorName
                publish (Event.SensorNameChangeCompleted event)

            | _ -> ()
        }
    
    let SubscribeTo publish (events : IObservable<Event.Event>) : IDisposable =
        let handle = handle publish
        events
        |> Observable.subscribe (fun event -> handle event |> Async.Start)
