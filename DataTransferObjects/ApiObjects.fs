namespace Jottai

module ApiObjects =

    type RefreshTokenRequest = 
        { Code : string
          RedirectUri : string }

    type AccessTokenRequest = 
        { RefreshToken : string
          RedirectUri : string }

    type RefreshToken = 
        { RefreshToken : string }

    type AccessToken = 
        { AccessToken : string
          Expires : System.DateTimeOffset }

    type DeviceDatum = 
        { propertyId : string
          propertyTypeId : string
          propertyName : string
          propertyDescription : string
          protocol : string
          unitOfMeasurement : string
          valueType : string
          value : string
          formattedValue : string
          minimumValue: string
          maximumValue: string }
    
    type DeviceData = 
        { gatewayId : string
          channel : string
          deviceId : string
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
        { SensorId : string
          MeasuredProperty : string
          Entries : SensorHistoryEntry list }

    type SensorState = 
        { DeviceGroupId : string
          DeviceId : string
          SensorId : string
          SensorName : string
          MeasuredProperty : string
          MeasuredValue : obj
          BatteryVoltage : float
          SignalStrength : float
          LastUpdated : System.DateTimeOffset
          LastActive : System.DateTimeOffset }

    type DevicePropertyState = 
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
