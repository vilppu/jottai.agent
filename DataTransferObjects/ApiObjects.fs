namespace Jottai

module ApiObjects =

    type PropertyType =
        | Voltage = 1
        | Rssi = 2
        | Temperature = 3
        | RelativeHumidity = 4
        | PresenceOfWater = 5
        | Contact = 6
        | Motion = 7
        | Luminance = 8
        | SeismicIntensity = 9
        | TwoWaySwitch = 10

    type Protocol = 
        | NotSpecified = 1
        | ZWave = 2
        | ZWavePlus = 3

    type ValueType =
        | Boolean = 1
        | Integer = 2
        | Decimal = 3

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
          valueType : ValueType          
          value : string
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
