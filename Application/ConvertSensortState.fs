namespace Jottai

module internal ConvertSensorState =
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols   
    
    let private FromStorable (storable : SensorStateStorage.StorableSensorState) : SensorState option =
        match Measurement.From storable.MeasuredProperty storable.MeasuredValue with
        | Some measurement ->
            (
            let batteryVoltage : Measurement.Voltage = storable.BatteryVoltage * 1.0<V>
            let signalStrength : Measurement.Rssi = storable.SignalStrength

            { SensorId = SensorId storable.SensorId
              DeviceGroupId = DeviceGroupId storable.DeviceGroupId
              DeviceId = DeviceId storable.DeviceId
              SensorName = SensorName storable.SensorName
              Measurement = measurement
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
                    SensorId = sensorState.SensorId.AsString
                    SensorName = sensorState.SensorName.AsString
                    MeasuredProperty = sensorState.Measurement |> Measurement.Name
                    MeasuredValue = sensorState.Measurement |> Measurement.Value
                    BatteryVoltage = float(sensorState.BatteryVoltage)
                    SignalStrength = sensorState.SignalStrength
                    LastUpdated = sensorState.LastUpdated
                    LastActive = sensorState.LastActive })
