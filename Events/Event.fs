namespace Jottai

module Event =

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
          Timestamp : System.DateTimeOffset }

    type SensorNameChanged = 
        { SensorId : SensorId
          DeviceGroupId : DeviceGroupId
          SensorName : SensorName }

    type DevicePropertyChangeRequest =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId          
          PropertyValue : DeviceProperty.DeviceProperty }

    type DevicePropertyNameChangeRequest =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId          
          PropertyName : PropertyName }

    type Event =
        | SubscribedToPushNotifications of SubscribedToPushNotifications
        | PushNotificationSubscriptionStored of SubscribedToPushNotifications
        | PushNotificationsSent of SensorState
        | SensorStateChanged of SensorStateChanged
        | SensorStateStored of SensorState
        | SensorNameChanged of SensorNameChanged
        | SensorNameStored of SensorNameChanged
        | DevicePropertyAvailable of DeviceProperty
        | DevicePropertyStored of DeviceProperty
        | DevicePropertyChangeRequested of DevicePropertyChangeRequest        
        | DevicePropertyNameChangeRequested of DevicePropertyNameChangeRequest
        | DevicePropertyNameChanged of DevicePropertyNameChangeRequest
        | PollingDevicePropertyChangeRequests

    let ToSensorStateUpdate (event : SensorStateChanged) : SensorStateUpdate = 
        { SensorId = event.SensorId
          DeviceGroupId = event.DeviceGroupId
          DeviceId = event.DeviceId
          Measurement = event.Measurement
          BatteryVoltage = event.BatteryVoltage
          SignalStrength = event.SignalStrength
          Timestamp = event.Timestamp }
