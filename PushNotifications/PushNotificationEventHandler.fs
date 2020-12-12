namespace Jottai

module PushNotificationEventHandler =

    open System
    open FSharp.Control.Reactive

    let handle httpSend publish (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SubscribedToPushNotifications subscribed ->
                do! PushNotificationSubscriptionStorage.StorePushNotificationSubscriptions subscribed.DeviceGroupId.AsString [subscribed.Subscription.Token]

            | Event.SensorStateChangeCompleted sensorState ->
                do! Action.SendNotifications httpSend sensorState

            | _ -> ()
        }
    
    let SubscribeTo httpSend publish (events : IObservable<Event.Event>) : IDisposable =
        let handle = handle httpSend publish
        events
        |> Observable.subscribe (fun event -> handle event |> Async.Start)
