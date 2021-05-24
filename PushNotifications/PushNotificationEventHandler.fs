namespace Jottai

module PushNotificationEventHandler =

    open System
    open FSharp.Control.Reactive

    let Handle httpSend publish (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SubscribedToPushNotifications subscribed ->
                let (DeviceGroupId deviceGroupId) = subscribed.DeviceGroupId
                do! PushNotificationSubscriptionStorage.StorePushNotificationSubscriptions deviceGroupId [subscribed.Subscription.Token]
                publish (Event.PushNotificationSubscriptionStored subscribed)

            | Event.SensorStateStored sensorState ->
                do! Action.SendSensorStateNotifications httpSend sensorState
                publish Event.PushNotificationsSent

            | Event.DevicePropertyStored devicePropertyState ->
                do! Action.SendDevicePropertyStateNotifications httpSend devicePropertyState
                publish Event.PushNotificationsSent

            | _ -> ()
        }
    
    let SubscribeTo httpSend publish (events : IObservable<Event.Event>) : IDisposable =
        let handle = Handle httpSend publish
        events
        |> Observable.subscribe (fun event -> handle event |> Async.Start)
