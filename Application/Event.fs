namespace Jottai

module internal Event =

    type SubscribedToPushNotifications =
        { DeviceGroupId : DeviceGroupId
          Subscription : Notification.Subscription }

    type SensorStateChanged = 
        { SensorId : SensorId
          DeviceGroupId : DeviceGroupId
          DeviceId : DeviceId
          Measurement : Measurement.Measurement
          BatteryVoltage : Measurement.Voltage
          SignalStrength : Measurement.Rssi
          Timestamp : System.DateTime }

    type SensorNameChanged = 
        { SensorId : SensorId
          DeviceGroupId : DeviceGroupId
          SensorName : string }

    type Event =
        | SubscribedToPushNotifications of SubscribedToPushNotifications
        | SensorStateChanged of SensorStateChanged
        | SensorNameChanged of SensorNameChanged    

    let private toSensorStateUpdate (event : SensorStateChanged) : SensorStateUpdate = 
        { SensorId = event.SensorId
          DeviceGroupId = event.DeviceGroupId
          DeviceId = event.DeviceId
          Measurement = event.Measurement
          BatteryVoltage = event.BatteryVoltage
          SignalStrength = event.SignalStrength
          Timestamp = event.Timestamp }

    let Store (event : Event) : Async<unit> =
        async {
            match event with
            | SubscribedToPushNotifications _ -> ()
            | SensorStateChanged sensorStateChanged ->
                if false then
                    let sensorStateUpdate = sensorStateChanged |> toSensorStateUpdate
                    do! Action.StoreSensorStateChangedEvent sensorStateUpdate
            | SensorNameChanged _ -> ()
        }

    let Send httpSend (event : Event) : Async<unit> =
        async {
            match event with
            | SubscribedToPushNotifications event ->
                do! PushNotificationSubscriptionStorage.StorePushNotificationSubscriptions event.DeviceGroupId.AsString [event.Subscription.Token]

            | SensorStateChanged event ->
                let sensorStateUpdate = event |> toSensorStateUpdate
                let! sensorState = Action.GetSensorState sensorStateUpdate
                let! sensorHistory = Action.GetSensorHistory sensorStateUpdate               
                do! Action.StoreSensorState sensorState
                do! Action.StoreSensorHistory sensorState sensorHistory
                do! Action.SendNotifications httpSend sensorState

            | SensorNameChanged event ->
                do! SensorStateStorage.StoreSensorName event.DeviceGroupId.AsString event.SensorId.AsString event.SensorName
        }
  