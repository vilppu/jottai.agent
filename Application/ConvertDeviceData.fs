namespace Jottai

module internal ConvertDeviceData =
    open System
    open System.Text.RegularExpressions    
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols    
    
    let private MeasuredPropertyName (deviceDatum : ApiObjects.DeviceDatum) : string =
        if String.IsNullOrEmpty(deviceDatum.propertyName)
        then ""
        else deviceDatum.propertyName.ToLower()
    
    let private ToRoundedNumericValue input : float option = 
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
    
    let private ToNumericValue input : float option= 
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
        
    let private ToMeasurement (deviceDatum : ApiObjects.DeviceDatum) : Measurement.Measurement option =
    
        match deviceDatum |> MeasuredPropertyName with
        | "rh" -> 
            match deviceDatum.formattedValue |> ToRoundedNumericValue with
            | Some value -> Some(Measurement.RelativeHumidity value)
            | None -> None
        | "temperature" -> 
            match deviceDatum.formattedValue |> ToRoundedNumericValue with
            | Some value -> Some(Measurement.Temperature(value * 1.0<C>))
            | None -> None
        | "detect" | "presenceofwater" -> 
            Some(Measurement.PresenceOfWater(if deviceDatum.value = "1" then Measurement.Present
                                    else Measurement.NotPresent))
        | "contact" -> 
            Some(Measurement.Contact(if deviceDatum.value = "1" then Measurement.Open
                            else Measurement.Closed))
        | "pir" | "motion" -> 
            Some(Measurement.Measurement.Motion(if deviceDatum.value = "1" then Measurement.Motion
                        else Measurement.NoMotion))
        | "voltage" -> 
            match deviceDatum.value |> ToNumericValue with
            | Some value -> Some(Measurement.Voltage(value * 1.0<V>))
            | None -> None
        | "rssi" -> 
            match deviceDatum.value |> ToNumericValue with
            | Some value -> Some(Measurement.Rssi(value))
            | None -> None
        | _ -> None
        
    let private ToBatteryVoltage (deviceData : ApiObjects.DeviceData) : Measurement.Voltage = 
        match deviceData.batteryVoltage |> ToNumericValue with
        | Some value -> 
            value * 1.0<V>
        | _ -> 0.0<V>
        
    let private ToRssi (deviceData : ApiObjects.DeviceData) : Measurement.Rssi= 
        match deviceData.rssi |> ToNumericValue with
        | Some value -> 
            value
        | _ -> 0.0
    
    let private ToMeasuredPropertyName (deviceDatum : ApiObjects.DeviceDatum) =
        if String.IsNullOrEmpty(deviceDatum.propertyName)
        then ""
        else deviceDatum.propertyName.ToLower()
    
    let private ToSensorStateUpdate
        (deviceGroupId : DeviceGroupId)
        (deviceData : ApiObjects.DeviceData)
        (deviceDatum : ApiObjects.DeviceDatum)
        : DeviceDataUpdate option =
        
        let measurementOption = ToMeasurement deviceDatum
        let timestamp =
            if String.IsNullOrWhiteSpace(deviceData.timestamp)
            then DateTimeOffset.UtcNow
            else DateTimeOffset.Parse(deviceData.timestamp)

        match measurementOption with
        | Some measurement ->
            let property = deviceDatum |> ToMeasuredPropertyName
            let deviceId = DeviceId deviceData.deviceId
            let sensorStateUpdate : SensorStateUpdate =
                { SensorId = SensorId (deviceId.AsString + "." + property)
                  DeviceGroupId = deviceGroupId
                  DeviceId = deviceId
                  Measurement = measurement
                  BatteryVoltage = ToBatteryVoltage deviceData
                  SignalStrength = ToRssi deviceData
                  Timestamp = timestamp }
            
            sensorStateUpdate |> SensorStateUpdate |> Some
        | None -> None

    let private ToDeviceProtocol (deviceProtocol : string) : DeviceProtocol =        
        match deviceProtocol with
            | "Z-Wave Plus" -> ZWavePlus
            | _ -> ProtocolNotSpecified        
    
    let private ToDeviceDataUpdate
        (deviceGroupId : DeviceGroupId)
        (deviceData : ApiObjects.DeviceData)
        (deviceDatum : ApiObjects.DeviceDatum)
        : (DeviceDataUpdate option) =

        match deviceDatum.protocol |> ToDeviceProtocol with
        | ZWavePlus -> ZWavePlus.ToDeviceDataUpdate deviceGroupId deviceData deviceDatum
        | ProtocolNotSpecified -> ToSensorStateUpdate deviceGroupId deviceData deviceDatum
    
    let ToDeviceDataUpdates (deviceGroupId : DeviceGroupId) (deviceData : ApiObjects.DeviceData)
        : DeviceDataUpdate list =
        let data = 
            match deviceData.data :> obj with
            | null -> list.Empty
            | _ -> deviceData.data

        data
        |> Seq.toList
        |> List.map (fun deviceDatum -> ToDeviceDataUpdate deviceGroupId deviceData deviceDatum)
        |> List.choose (id)
