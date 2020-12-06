namespace Jottai

[<AutoOpen>]
module Sensors =
    open System

    type DeviceGroupId = 
        | DeviceGroupId of string
        member this.AsString = 
            let (DeviceGroupId unwrapped) = this
            unwrapped
    
    type DeviceId = 
        | DeviceId of string
        member this.AsString = 
            let (DeviceId unwrapped) = this
            unwrapped
    
    type SensorId = 
        | SensorId of string
        member this.AsString = 
            let (SensorId unwrapped) = this
            unwrapped

    type SensorHistoryEntry = 
        { MeasuredValue : obj
          Timestamp : DateTime }

    type SensorHistory = 
        { SensorId : string
          MeasuredProperty : string
          Entries : SensorHistoryEntry list }

    type SensorState = 
        { SensorId : SensorId
          DeviceGroupId : DeviceGroupId
          DeviceId : DeviceId
          SensorName : string
          Measurement : Measurement.Measurement
          BatteryVoltage : Measurement.Voltage
          SignalStrength : Measurement.Rssi
          LastUpdated : System.DateTime
          LastActive : System.DateTime }

    type SensorStateUpdate = 
        { SensorId : SensorId
          DeviceGroupId : DeviceGroupId
          DeviceId : DeviceId
          Measurement : Measurement.Measurement
          BatteryVoltage : Measurement.Voltage
          SignalStrength : Measurement.Rssi
          Timestamp : System.DateTime }

    let EmptySensorHistory : SensorHistory = 
        { SensorId = ""
          MeasuredProperty = ""
          Entries = List.empty }