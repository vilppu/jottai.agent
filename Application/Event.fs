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

    let ToSensorStateUpdate (event : SensorStateChanged) : SensorStateUpdate = 
        { SensorId = event.SensorId
          DeviceGroupId = event.DeviceGroupId
          DeviceId = event.DeviceId
          Measurement = event.Measurement
          BatteryVoltage = event.BatteryVoltage
          SignalStrength = event.SignalStrength
          Timestamp = event.Timestamp }