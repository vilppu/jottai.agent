namespace Jottai

module internal ConvertSensorStateUpdate =
    open System
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

    let private IsEmpty source =
        if String.IsNullOrWhiteSpace(source)
        then true
        else false

    let private LowerCase source =
        if String.IsNullOrWhiteSpace(source)
        then ""
        else source.ToLower()
    
    let private ParseBoolean (value : string) : bool option = 
        let valueIsBoolean, parsedValue = Boolean.TryParse(value)
        if valueIsBoolean
        then Some parsedValue
        else None
        
    let private ParseInteger (value : string) : int option = 
        let valueIsInteger, parsedValue = Int32.TryParse(value)
        if valueIsInteger
        then Some parsedValue
        else None
    
    let private ParseDecimal (value : string) : float option = 
        let valueIsDecimal, parsedValue = Double.TryParse(value, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture)
        if valueIsDecimal
        then Some parsedValue
        else None
    
    let private ToBooleanValue (deviceDatum : ApiObjects.DeviceDatum) : bool option = 
        match deviceDatum.valueType with
        | ApiObjects.ValueType.Boolean -> deviceDatum.value |> ParseBoolean
        | ApiObjects.ValueType.Integer ->
            match deviceDatum.value |> ParseInteger with
            | Some integer -> Some(if integer = 0 then false else true)
            | _ -> None
        | _ -> None        
    
    let private ToRoundedNumericValue (deviceDatum : ApiObjects.DeviceDatum) : float option = 
        match deviceDatum.valueType with
        | ApiObjects.ValueType.Integer ->
            match deviceDatum.value |> ParseInteger with
            | Some integer -> Some (float(integer))
            | _ -> None
        | ApiObjects.ValueType.Decimal ->            
            match deviceDatum.value |> ParseDecimal with
            | Some decimal -> Some (float((int(decimal))))
            | _ -> None            
        | _ -> None
    
    let private ToNumericValue (deviceDatum : ApiObjects.DeviceDatum) : float option= 
        match deviceDatum.valueType with
        | ApiObjects.ValueType.Integer ->
            match deviceDatum.value |> ParseInteger with
            | Some integer -> Some (float(integer))
            | _ -> None
        | ApiObjects.ValueType.Decimal -> deviceDatum.value |> ParseDecimal
        | _ -> None
        
    let private ToMeasurement (deviceDatum : ApiObjects.DeviceDatum) : Measurement.Measurement option =
        match deviceDatum.propertyType with
        | ApiObjects.PropertyType.Voltage ->
            match deviceDatum |> ToNumericValue with
            | Some value -> Some(Measurement.Voltage(value * 1.0<V>))
            | _ -> None
        | ApiObjects.PropertyType.Rssi ->
            match deviceDatum |> ToNumericValue with
            | Some value -> Some(Measurement.Rssi(value))
            | _ -> None
        | ApiObjects.PropertyType.Temperature ->
            match deviceDatum |> ToRoundedNumericValue with
            | Some value -> Some(Measurement.Temperature(value * 1.0<C>))
            | _ -> None
        | ApiObjects.PropertyType.RelativeHumidity ->
            match deviceDatum |> ToRoundedNumericValue with
            | Some value -> Some(Measurement.RelativeHumidity value)
            | _ -> None
        | ApiObjects.PropertyType.PresenceOfWater ->
            match deviceDatum |> ToBooleanValue with
            | Some value -> Some(Measurement.PresenceOfWater(if value then Measurement.Present else Measurement.NotPresent))
            | _ -> None
        | ApiObjects.PropertyType.Contact -> 
            match deviceDatum |> ToBooleanValue with
            | Some value -> Some(Measurement.Contact(if value then Measurement.Open else Measurement.Closed))
            | _ -> None
        | ApiObjects.PropertyType.Motion -> 
            match deviceDatum |> ToBooleanValue with
            | Some value -> Some(Measurement.Measurement.Motion(if value then Measurement.Motion else Measurement.NoMotion))
            | _ -> None
        | ApiObjects.PropertyType.Luminance ->
            match deviceDatum |> ToRoundedNumericValue with
            | Some value -> Some(Measurement.Luminance(value * 1.0<lx>))
            | _ -> None
        | ApiObjects.PropertyType.SeismicIntensity ->
            match deviceDatum |> ToRoundedNumericValue with
            | Some value -> Some(Measurement.SeismicIntensity(value * 1.0<Measurement.MM>))
            | _ -> None
        | ApiObjects.PropertyType.Acceleration ->
            match deviceDatum |> ToRoundedNumericValue with
            | Some value -> Some(Measurement.Acceleration(value * 1.0<m/s^2>))
            | _ -> None
        | _ -> None
        
    let private ToBatteryVoltage (deviceData : ApiObjects.DeviceData) : Measurement.Voltage = 
        match deviceData.batteryVoltage |> ParseDecimal with
        | Some value -> 
            value * 1.0<V>
        | _ -> 0.0<V>
        
    let private ToRssi (deviceData : ApiObjects.DeviceData) : Measurement.Rssi= 
        match deviceData.rssi |> ParseDecimal with
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
