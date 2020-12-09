namespace Jottai

module internal EventHandler =

    open System
    open FSharp.Control.Reactive

    let handle httpSend publish (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SubscribedToPushNotifications event ->
                do! PushNotificationSubscriptionStorage.StorePushNotificationSubscriptions event.DeviceGroupId.AsString [event.Subscription.Token]

            | Event.SensorStateChanged event ->
                let sensorStateUpdate = event |> Event.ToSensorStateUpdate
                let! sensorState = Action.GetSensorState sensorStateUpdate
                let! sensorHistory = Action.GetSensorHistory sensorStateUpdate               
                do! Action.StoreSensorState sensorState
                do! Action.StoreSensorHistory sensorState sensorHistory
                do! Action.SendNotifications httpSend sensorState
                publish sensorState

            | Event.SensorNameChanged event ->
                do! SensorStateStorage.StoreSensorName event.DeviceGroupId.AsString event.SensorId.AsString event.SensorName
        }
    
    let SubscribeTo httpSend publish (events : IObservable<Event.Event>) : IDisposable =
        let handle = handle httpSend publish
        events
        |> Observable.subscribe (fun event -> handle event |> Async.Start)

