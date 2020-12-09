namespace Jottai

[<AutoOpen>]
module internal ConvertSensorData =
    open System
    open System.Text.RegularExpressions    
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    open ApiObjects
    
    let private MeasuredPropertyName (datum : DeviceDatum) : string =
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
        
    let private SensorDatumToMeasurement (datum : DeviceDatum) : Measurement.Measurement option =
    
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
        
    let private ToBatteryVoltage (deviceData : DeviceData) : Measurement.Voltage = 
        match deviceData.batteryVoltage |> toNumericValue with
        | Some value -> 
            value * 1.0<V>
        | _ -> 0.0<V>
        
    let private ToRssi (deviceData : DeviceData) : Measurement.Rssi= 
        match deviceData.rssi |> toNumericValue with
        | Some value -> 
            value
        | _ -> 0.0
    
    let private measuredPropertyName (datum : DeviceDatum) =
        if System.String.IsNullOrEmpty(datum.name)
        then ""
        else datum.name.ToLower()
    
    let private toSensorStateUpdate
        (deviceGroupId : DeviceGroupId)
        (deviceData : DeviceData)
        (datum : DeviceDatum)
        : Option<SensorStateUpdate> =
        
        let measurementOption = SensorDatumToMeasurement datum
        let timestamp =
            if String.IsNullOrWhiteSpace(deviceData.timestamp)
            then DateTime.UtcNow
            else DateTime.Parse(deviceData.timestamp)

        match measurementOption with
        | Some measurement ->
            let property = datum |> measuredPropertyName
            let deviceId = DeviceId deviceData.deviceId
            let sensorStateUpdate : SensorStateUpdate =
                { SensorId = SensorId (deviceId.AsString + "." + property)
                  DeviceGroupId = deviceGroupId
                  DeviceId = deviceId
                  Measurement = measurement
                  BatteryVoltage = ToBatteryVoltage deviceData
                  SignalStrength = ToRssi deviceData
                  Timestamp = timestamp }

            Some sensorStateUpdate
        | None -> None
    
    let ToSensorStateUpdates (deviceGroupId : DeviceGroupId) (deviceData : DeviceData)
        : SensorStateUpdate list =
        deviceData.data
        |> Seq.toList
        |> List.map (fun datum -> toSensorStateUpdate deviceGroupId deviceData datum)
        |> List.choose (id)
