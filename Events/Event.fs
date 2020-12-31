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

    type DevicePropertyChanged =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId
          PropertyType : PropertyType
          PropertyName : PropertyName
          PropertyDescription : PropertyDescription
          PropertyValue : DeviceProperty.DeviceProperty
          Protocol : DeviceProtocol
          Timestamp : System.DateTimeOffset }

    type DevicePropertyChangeRequest =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId          
          PropertyValue : DeviceProperty.DeviceProperty }

    type DevicePropertyNameChanged =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId          
          PropertyName : PropertyName }

    type Event =
        | SubscribedToPushNotifications of SubscribedToPushNotifications
        | PushNotificationSubscriptionStored of SubscribedToPushNotifications
        | PushNotificationsSent
        | SensorStateChanged of SensorStateChanged
        | SensorStateStored of SensorState
        | SensorNameChanged of SensorNameChanged
        | SensorNameStored of SensorNameChanged
        | DevicePropertyChanged of DevicePropertyChanged
        | DevicePropertyStored of DevicePropertyState
        | DevicePropertyChangeRequested of DevicePropertyChangeRequest
        | PollingDevicePropertyChangeRequests
        | DevicePropertyNameChanged of DevicePropertyNameChanged
        | DevicePropertyNameStored of DevicePropertyNameChanged

    let ToSensorStateUpdate (event : SensorStateChanged) : SensorStateUpdate = 
        { SensorId = event.SensorId
          DeviceGroupId = event.DeviceGroupId
          DeviceId = event.DeviceId
          Measurement = event.Measurement
          BatteryVoltage = event.BatteryVoltage
          SignalStrength = event.SignalStrength
          Timestamp = event.Timestamp }

    let ToDevicePropertyUpdate (event : DevicePropertyChanged) : DevicePropertyUpdate = 
        { DeviceGroupId = event.DeviceGroupId
          GatewayId = event.GatewayId
          DeviceId = event.DeviceId
          PropertyId = event.PropertyId
          PropertyType = event.PropertyType
          PropertyName = event.PropertyName
          PropertyDescription = event.PropertyDescription
          PropertyValue = event.PropertyValue
          Protocol = event.Protocol
          Timestamp = event.Timestamp }