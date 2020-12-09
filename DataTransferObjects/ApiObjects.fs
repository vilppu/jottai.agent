namespace Jottai

module ApiObjects =

    type DeviceDatum = 
        { name : string
          unit : string
          value : string          
          formattedValue : string }
          
    type DeviceCommand = 
        { id: string
          name : string }
    
    type DeviceData = 
        { gatewayId : string
          channel : string
          deviceId : string
          data : DeviceDatum list
          availableCommands : DeviceCommand list
          batteryVoltage : string
          rssi : string
          timestamp : string }
          
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
