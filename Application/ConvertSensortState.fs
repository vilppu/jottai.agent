namespace Jottai

module internal ConvertSensorState =
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols  

    let private ProtocolToApiObject (deviceProtocol : DeviceProtocol ) : string =
        match deviceProtocol with
        | ZWave -> "Z-Wave"
        | ZWavePlus -> "Z-Wave Plus"
        | NotSpecified -> ""

    let private ProtocolFromStorable (storableProtocol : string ) : DeviceProtocol =
        match storableProtocol with
        | "Z-Wave" -> ZWave
        | "Z-Wave Plus" -> ZWavePlus
        | _ -> NotSpecified
    
    let private FromStorable (storable : SensorStateStorage.StorableSensorState) : SensorState option =
        match Measurement.From storable.PropertyType storable.PropertyValue with
        | Some measurement ->
            (
            let batteryVoltage : Measurement.Voltage = storable.BatteryVoltage * 1.0<V>
            let signalStrength : Measurement.Rssi = storable.SignalStrength

            { DeviceGroupId = DeviceGroupId storable.DeviceGroupId
              GatewayId = GatewayId storable.GatewayId
              PropertyId = PropertyId storable.PropertyId
              DeviceId = DeviceId storable.DeviceId
              PropertyName = PropertyName storable.PropertyName
              PropertyDescription = PropertyDescription storable.PropertyDescription
              Measurement = measurement
              Protocol = storable.Protocol |> ProtocolFromStorable
              BatteryVoltage = batteryVoltage
              SignalStrength = signalStrength
              LastActive = storable.LastActive
              LastUpdated = storable.LastUpdated }
            |> Some
            )
        | None -> None
    
    let FromStorables (storable : seq<SensorStateStorage.StorableSensorState>) : SensorState list =        
        let statuses =
            if storable :> obj |> isNull then
                List.empty
            else
                storable
                |> Seq.toList
                |> List.map FromStorable
                |> List.choose id
        statuses
              
    let ToApiObjects (statuses : SensorState list) : ApiObjects.SensorState list = 
              statuses
              |> List.map (fun sensorState ->            
                  { DeviceGroupId = sensorState.DeviceGroupId.AsString
                    DeviceId = sensorState.DeviceId.AsString
                    PropertyId = sensorState.PropertyId.AsString
                    PropertyName = sensorState.PropertyName.AsString
                    MeasuredProperty = sensorState.Measurement |> Measurement.Name
                    MeasuredValue = sensorState.Measurement |> Measurement.Value
                    Protocol = sensorState.Protocol |> ProtocolToApiObject
                    BatteryVoltage = float(sensorState.BatteryVoltage)
                    SignalStrength = sensorState.SignalStrength
                    LastUpdated = sensorState.LastUpdated
                    LastActive = sensorState.LastActive })
