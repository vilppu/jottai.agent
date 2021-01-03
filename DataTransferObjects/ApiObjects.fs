namespace Jottai

module ApiObjects =

    type PropertyType = 
        | Sensor = 0
        | TwoWaySwitch = 1

    type Protocol = 
        | NotSpecified = 0
        | ZWave = 1
        | ZWavePlus = 2

    type RefreshTokenRequest = 
        { Code : string
          RedirectUri : string }

    type AccessTokenRequest = 
        { RefreshToken : string }

    type RefreshToken = 
        { RefreshToken : string }

    type AccessToken = 
        { AccessToken : string
          Expires : System.DateTimeOffset }

    type DeviceDatum = 
        { propertyId : string
          propertyType : PropertyType
          propertyName : string
          propertyDescription : string
          unitOfMeasurement : string
          valueType : string
          value : string
          formattedValue : string
          minimumValue: string
          maximumValue: string }
    
    type DeviceData = 
        { gatewayId : string
          deviceId : string
          protocol : Protocol
          manufacturerName : string
          deviceName : string
          data : DeviceDatum list
          batteryVoltage : string
          rssi : string
          timestamp : string }

    type DevicePropertyChangeRequest = 
        { GatewayId : string
          DeviceId : string
          PropertyId : string          
          PropertyValue : string }

    type SensorHistoryEntry = 
        { MeasuredValue : obj
          Timestamp : System.DateTimeOffset }
    
    type SensorHistory = 
        { PropertyId : string
          MeasuredProperty : string
          Entries : SensorHistoryEntry list }

    type SensorState = 
        { DeviceGroupId : string
          DeviceId : string
          PropertyId : string
          PropertyName : string
          MeasuredProperty : string
          MeasuredValue : obj
          Protocol : string
          BatteryVoltage : float
          SignalStrength : float
          LastUpdated : System.DateTimeOffset
          LastActive : System.DateTimeOffset }

    type DeviceProperty = 
        { DeviceGroupId : string
          GatewayId : string
          DeviceId : string
          PropertyId : string
          PropertyType : string
          PropertyName : string
          PropertyDescription : string
          PropertyValue : obj
          Protocol : string
          LastUpdated : System.DateTimeOffset
          LastActive : System.DateTimeOffset }

    type Measurement = 
        { Name : string
          Value : obj }
