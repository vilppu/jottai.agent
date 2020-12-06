namespace Jottai

[<AutoOpen>]
module internal ConvertSensorData =
    open System
    open System.Text.RegularExpressions    
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    open ApiObjects
    
    type private GatewayEvent = 
        | GatewayUpEvent of SensorData
        | GatewayDownEvent of SensorData
        | GatewayActiveOnChannelEvent of SensorData
        | SensorUpEvent of SensorData
        | SensorDataEvent of SensorData
    
    let private MeasuredPropertyName (datum : SensorDatum) : string =
        if String.IsNullOrEmpty(datum.name)
        then ""
        else datum.name.ToLower()
    
    let private toRoundedNumericValue input : float option = 
        match input with
        | null -> None
        | _ -> 
            let (|FirstRegexGroup|_|) pattern input = 
                let m = Regex.Match(input, pattern)
                if (m.Success) then Some m.Groups.[1].Value
                else None
            match input with
            | FirstRegexGroup "(\d+(?:\.\d+)?)" value -> Some(System.Math.Round(float (value)))
            | _ -> None
    
    let private toNumericValue input : float option= 
        match input with
        | null -> None
        | _ -> 
            let (|FirstRegexGroup|_|) pattern input = 
                let m = Regex.Match(input, pattern)
                if (m.Success) then Some m.Groups.[1].Value
                else None
            match input with
            | FirstRegexGroup "(\d+(?:\.\d+)?)" value -> Some(float (value))
            | _ -> None
        
    let private SensorDatumToMeasurement (datum : SensorDatum) : Measurement.Measurement option =
    
        match datum |> MeasuredPropertyName with
        | "rh" -> 
            match datum.formattedValue |> toRoundedNumericValue with
            | Some value -> Some(Measurement.RelativeHumidity value)
            | None -> None
        | "temperature" -> 
            match datum.formattedValue |> toRoundedNumericValue with
            | Some value -> Some(Measurement.Temperature(value * 1.0<C>))
            | None -> None
        | "detect" | "presenceofwater" -> 
            Some(Measurement.PresenceOfWater(if datum.value = "1" then Measurement.Present
                                    else Measurement.NotPresent))
        | "contact" -> 
            Some(Measurement.Contact(if datum.value = "1" then Measurement.Open
                            else Measurement.Closed))
        | "pir" | "motion" -> 
            Some(Measurement.Measurement.Motion(if datum.value = "1" then Measurement.Motion
                        else Measurement.NoMotion))
        | "voltage" -> 
            match datum.value |> toNumericValue with
            | Some value -> Some(Measurement.Voltage(value * 1.0<V>))
            | None -> None
        | "rssi" -> 
            match datum.value |> toNumericValue with
            | Some value -> Some(Measurement.Rssi(value))
            | None -> None
        | _ -> None
        
    let private ToBatteryVoltage (sensorData : SensorData) : Measurement.Voltage = 
        match sensorData.batteryVoltage |> toNumericValue with
        | Some value -> 
            value * 1.0<V>
        | _ -> 0.0<V>
        
    let private ToRssi (sensorData : SensorData) : Measurement.Rssi= 
        match sensorData.rssi |> toNumericValue with
        | Some value -> 
            value
        | _ -> 0.0    
        
    let private ToGatewayEvent(sensorData : SensorData) : GatewayEvent = 
        match sensorData.event with
        | "gateway up" -> GatewayUpEvent sensorData
        | "gateway down" -> GatewayDownEvent sensorData
        | "gateway active" -> GatewayActiveOnChannelEvent sensorData
        | "sensor up" -> SensorUpEvent sensorData
        | "sensor data" -> SensorDataEvent sensorData
        | _ -> failwith ("unknown sensor event: " + sensorData.event)
    
    let private measuredPropertyName (datum : SensorDatum) =
        if System.String.IsNullOrEmpty(datum.name)
        then ""
        else datum.name.ToLower()
    
    let private toSensorStateUpdate
        (deviceGroupId : DeviceGroupId)
        (sensorData : SensorData)
        (datum : SensorDatum)
        (timestamp : System.DateTime)
        : Option<SensorStateUpdate> =
        
        let measurementOption = SensorDatumToMeasurement datum

        match measurementOption with
        | Some measurement ->
            let property = datum |> measuredPropertyName
            let deviceId = DeviceId sensorData.deviceId
            let sensorStateUpdate : SensorStateUpdate =
                { SensorId = SensorId (deviceId.AsString + "." + property)
                  DeviceGroupId = deviceGroupId
                  DeviceId = deviceId
                  Measurement = measurement
                  BatteryVoltage = ToBatteryVoltage sensorData
                  SignalStrength = ToRssi sensorData
                  Timestamp = timestamp }

            Some sensorStateUpdate
        | None -> None

    let private toChangeSensorStateCommands (deviceGroupId : DeviceGroupId) (sensorData : SensorData) timestamp : SensorStateUpdate list =
        sensorData.data
        |> Seq.toList
        |> List.map (fun datum -> toSensorStateUpdate deviceGroupId sensorData datum timestamp)
        |> List.choose (id)
    
    let ToSensorStateUpdates (deviceGroupId : DeviceGroupId) (sensorData : SensorData) : SensorStateUpdate list = 
        let timestamp = System.DateTime.UtcNow
        let gatewayEvent = ToGatewayEvent sensorData
        match gatewayEvent with
        | GatewayEvent.SensorDataEvent sensorData ->
            toChangeSensorStateCommands deviceGroupId sensorData timestamp
        | _ -> []
  