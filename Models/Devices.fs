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
    
    type PropertyDescription = 
        | PropertyDescription of string
        member this.AsString = 
            let (PropertyDescription unwrapped) = this
            unwrapped
            
    let ValidatePropertyName value =
        let value =
            if System.String.IsNullOrWhiteSpace value
            then ""
            else value.Trim()
        
        if value.Length <= 64
        then PropertyName value |> Some
        else None

    type DeviceProtocol =
        | NotSpecified
        | ZWave
        | ZWavePlus

    type SensorHistoryEntry = 
        { MeasuredValue : obj
          Timestamp : System.DateTimeOffset }

    type SensorHistory = 
        { PropertyId : string
          MeasuredProperty : string
          Entries : SensorHistoryEntry list }

    let EmptySensorHistory : SensorHistory = 
        { PropertyId = ""
          MeasuredProperty = ""
          Entries = List.empty }

    type SensorState = 
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
          LastUpdated : System.DateTimeOffset
          LastActive : System.DateTimeOffset }

    type DevicePropertyState =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId
          PropertyName : PropertyName
          PropertyDescription : PropertyDescription          
          PropertyValue : DeviceProperty.DeviceProperty
          Protocol : DeviceProtocol
          LastUpdated : System.DateTimeOffset 
          LastActive : System.DateTimeOffset }

    type SensorStateUpdate = 
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

    type DevicePropertyStateUpdate =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId
          PropertyName : PropertyName
          PropertyDescription : PropertyDescription
          PropertyValue : DeviceProperty.DeviceProperty
          Protocol : DeviceProtocol
          Timestamp : System.DateTimeOffset }

    type DeviceDataUpdate =
        | SensorStateUpdate of SensorStateUpdate
        | DevicePropertyUpdate of DevicePropertyStateUpdate
