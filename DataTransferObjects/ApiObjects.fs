namespace Jottai

module ApiObjects =

    type SensorDatum = 
        { name : string
          value : string
          scale : int
          formattedValue : string }
    
    type SensorData = 
        { event : string
          gatewayId : string
          channel : string
          deviceId : string
          data : SensorDatum list
          batteryVoltage : string
          rssi : string }
          
    type SensorHistoryEntry = 
        { MeasuredValue : obj
          Timestamp : System.DateTime }
    
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
          LastUpdated : System.DateTime
          LastActive : System.DateTime }
    
    type Measurement = 
        { Name : string
          Value : obj }
