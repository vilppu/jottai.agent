namespace Jottai

module PushNotificationEventHandler =

    let Handle httpSend (publish : Event.Event->Async<unit>) (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SubscribedToPushNotifications subscribed ->
                let (DeviceGroupId deviceGroupId) = subscribed.DeviceGroupId
                do! PushNotificationSubscriptionStorage.StorePushNotificationSubscriptions deviceGroupId [subscribed.Subscription.Token]
                do! publish (Event.PushNotificationSubscriptionStored subscribed)

            | Event.SensorStateStored sensorState ->
                do! Action.SendSensorStateNotifications httpSend sensorState
                do! publish Event.PushNotificationsSent

            | Event.DevicePropertyStored devicePropertyState ->
                do! Action.SendDevicePropertyStateNotifications httpSend devicePropertyState
                do! publish Event.PushNotificationsSent

            | _ -> ()
        }