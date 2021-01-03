namespace Jottai

module Event =

    type SubscribedToPushNotifications =
        { DeviceGroupId : DeviceGroupId
          Subscription : Notification.Subscription }

    type SensorStateChanged = 
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId          
          PropertyName : PropertyName
          PropertyDescription : PropertyDescription
          Measurement : Measurement.Measurement
          Protocol : DeviceProtocol
          BatteryVoltage : Measurement.Voltage
          SignalStrength : Measurement.Rssi
          Timestamp : System.DateTimeOffset }

    type SensorNameChanged = 
        { PropertyId : PropertyId
          DeviceGroupId : DeviceGroupId
          PropertyName : PropertyName }

    type DevicePropertyChanged =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId          
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
        { GatewayId = event.GatewayId
          PropertyId = event.PropertyId
          PropertyName = event.PropertyName
          PropertyDescription = event.PropertyDescription
          DeviceGroupId = event.DeviceGroupId
          DeviceId = event.DeviceId
          Measurement = event.Measurement
          Protocol = event.Protocol
          BatteryVoltage = event.BatteryVoltage
          SignalStrength = event.SignalStrength
          Timestamp = event.Timestamp }

    let ToDevicePropertyUpdate (event : DevicePropertyChanged) : DevicePropertyStateUpdate = 
        { GatewayId = event.GatewayId
          PropertyId = event.PropertyId
          PropertyName = event.PropertyName
          PropertyDescription = event.PropertyDescription
          DeviceGroupId = event.DeviceGroupId
          DeviceId = event.DeviceId
          PropertyValue = event.PropertyValue
          Protocol = event.Protocol
          Timestamp = event.Timestamp }