namespace Jottai

[<AutoOpen>]
module Devices =

    type DeviceGroupId = 
        | DeviceGroupId of string
        member this.AsString = 
            let (DeviceGroupId unwrapped) = this
            unwrapped
    
    type GatewayId = 
        | GatewayId of string
        member this.AsString = 
            let (GatewayId unwrapped) = this
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
    
    type SensorName = 
        | SensorName of string
        member this.AsString = 
            let (SensorName unwrapped) = this
            unwrapped
            
    let ValidateSensorName value =
        let value =
            if System.String.IsNullOrWhiteSpace value
            then ""
            else value.Trim()
        
        if value.Length <= 64
        then SensorName value |> Some
        else None
    
    type PropertyId = 
        | PropertyId of string
        member this.AsString = 
            let (PropertyId unwrapped) = this
            unwrapped
    
    type PropertyName = 
        | PropertyName of string
        member this.AsString = 
            let (PropertyName unwrapped) = this
            unwrapped
            
    let ValidatePropertyName value =
        let value =
            if System.String.IsNullOrWhiteSpace value
            then ""
            else value.Trim()
        
        if value.Length <= 64
        then PropertyName value |> Some
        else None
    
    type PropertyDescription = 
        | PropertyDescription of string
        member this.AsString = 
            let (PropertyDescription unwrapped) = this
            unwrapped
            
    type PropertyType= 
        | BinarySwitch  
    
    type PropertyTypeName = 
        | PropertyTypeName of string
        member this.AsString = 
            let (PropertyTypeName unwrapped) = this
            unwrapped
    
    type CommandName = 
        | CommandName of string
        member this.AsString = 
            let (CommandName unwrapped) = this
            unwrapped
    
    type CommandDescription = 
        | CommandDescription of string
        member this.AsString = 
            let (CommandDescription unwrapped) = this
            unwrapped

    type DeviceProtocol =
        | ZWavePlus
        | ProtocolNotSpecified

    type SensorHistoryEntry = 
        { MeasuredValue : obj
          Timestamp : System.DateTimeOffset }

    type SensorHistory = 
        { SensorId : string
          MeasuredProperty : string
          Entries : SensorHistoryEntry list }

    type SensorState = 
        { DeviceGroupId : DeviceGroupId
          DeviceId : DeviceId
          SensorId : SensorId
          SensorName : SensorName
          Measurement : Measurement.Measurement
          BatteryVoltage : Measurement.Voltage
          SignalStrength : Measurement.Rssi
          LastUpdated : System.DateTimeOffset
          LastActive : System.DateTimeOffset }

    let EmptySensorHistory : SensorHistory = 
        { SensorId = ""
          MeasuredProperty = ""
          Entries = List.empty }

    type DevicePropertyState =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId
          PropertyType : PropertyType
          PropertyName : PropertyName
          PropertyDescription : PropertyDescription
          PropertyValue : DeviceProperty.DeviceProperty
          Protocol : DeviceProtocol
          LastUpdated : System.DateTimeOffset 
          LastActive : System.DateTimeOffset }

    type SensorStateUpdate = 
        { DeviceGroupId : DeviceGroupId
          DeviceId : DeviceId
          SensorId : SensorId
          Measurement : Measurement.Measurement
          BatteryVoltage : Measurement.Voltage
          SignalStrength : Measurement.Rssi
          Timestamp : System.DateTimeOffset }

    type DevicePropertyUpdate =
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

    type DeviceDataUpdate =
        | SensorStateUpdate of SensorStateUpdate
        | DevicePropertyUpdate of DevicePropertyUpdate
