namespace Jottai

module PushNotificationEventHandler =

    open System
    open FSharp.Control.Reactive

    let Handle httpSend publish (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SubscribedToPushNotifications subscribed ->
                do! PushNotificationSubscriptionStorage.StorePushNotificationSubscriptions subscribed.DeviceGroupId.AsString [subscribed.Subscription.Token]
                publish (Event.PushNotificationSubscriptionStored subscribed)

            | Event.SensorStateStored sensorState ->
                do! Action.SendNotifications httpSend sensorState
                publish (Event.PushNotificationsSent sensorState)

            | _ -> ()
        }
    
    let SubscribeTo httpSend publish (events : IObservable<Event.Event>) : IDisposable =
        let handle = Handle httpSend publish
        events
        |> Observable.subscribe (fun event -> handle event |> Async.Start)
