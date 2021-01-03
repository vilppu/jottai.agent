namespace Jottai

module internal ConvertSensorStateUpdate =
    open System
    open System.Text.RegularExpressions    
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

    let private IsEmpty source =
        if String.IsNullOrWhiteSpace(source)
        then true
        else false

    let private LowerCase source =
        if String.IsNullOrWhiteSpace(source)
        then ""
        else source.ToLower()
    
    let private MeasuredPropertyName (deviceDatum : ApiObjects.DeviceDatum) : string =
        if deviceDatum.propertyName |> IsEmpty
        then ""
        else deviceDatum.propertyName |> LowerCase
    
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
        else deviceDatum.propertyName |>  LowerCase
    
    let FromDeviceData
        (deviceGroupId : DeviceGroupId)
        (deviceData : ApiObjects.DeviceData)
        (deviceDatum : ApiObjects.DeviceDatum)
        : DeviceDataUpdate option =
        
        let measurementOption = ToMeasurement deviceDatum
        let timestamp =
            if deviceData.timestamp |> IsEmpty
            then DateTimeOffset.UtcNow
            else DateTimeOffset.Parse(deviceData.timestamp)

        match measurementOption with
        | Some measurement ->
            let property = deviceDatum |> ToMeasuredPropertyName
            let deviceId = DeviceId deviceData.deviceId
            let sensorStateUpdate : SensorStateUpdate =
                { DeviceGroupId = deviceGroupId
                  GatewayId = GatewayId deviceData.deviceId
                  DeviceId = deviceId
                  PropertyId = PropertyId (deviceId.AsString + "." + property)
                  PropertyName = PropertyName ""
                  PropertyDescription = PropertyDescription ""
                  Measurement = measurement
                  Protocol = deviceData.protocol |> Convert.ProtocolFromApiObject
                  BatteryVoltage = ToBatteryVoltage deviceData
                  SignalStrength = ToRssi deviceData
                  Timestamp = timestamp }
            
            sensorStateUpdate |> SensorStateUpdate |> Some
        | None -> None

    let ToStorable (update : SensorStateUpdate) : SensorEventStorage.StorableSensorEvent  =            
            { Id = MongoDB.Bson.ObjectId.Empty
              DeviceGroupId =  update.DeviceGroupId.AsString
              DeviceId = update.DeviceId.AsString
              PropertyId = update.PropertyId.AsString
              PropertyType = update.Measurement |> Measurement.Name
              PropertyValue = update.Measurement |> Measurement.Value
              Voltage = (float)update.BatteryVoltage
              SignalStrength = (float)update.SignalStrength
              Timestamp = update.Timestamp }
