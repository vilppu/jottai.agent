namespace Jottai

[<AutoOpen>]
module internal ConvertStorableSensorState =
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols   
    
    let private fromStorable (storable : SensorStateStorage.StorableSensorState) : SensorState =
        let measurement = Measurement.From storable.MeasuredProperty storable.MeasuredValue
        let batteryVoltage : Measurement.Voltage = storable.BatteryVoltage * 1.0<V>
        let signalStrength : Measurement.Rssi = storable.SignalStrength

        { SensorId = SensorId storable.SensorId
          DeviceGroupId = DeviceGroupId storable.DeviceGroupId
          DeviceId = DeviceId storable.DeviceId
          SensorName = storable.SensorName
          Measurement = measurement
          BatteryVoltage = batteryVoltage
          SignalStrength = signalStrength
          LastActive = storable.LastActive
          LastUpdated = storable.LastUpdated }
    
    let FromStorables (storable : seq<SensorStateStorage.StorableSensorState>) : SensorState list =        
        let statuses =
            if storable :> obj |> isNull then
                List.empty
            else
                storable
                |> Seq.toList
                |> List.map fromStorable
        statuses

    let UpdateToStorable (update : SensorStateUpdate) : SensorEventStorage.StorableSensorEvent  =            
            { Id = MongoDB.Bson.ObjectId.Empty
              DeviceGroupId =  update.DeviceGroupId.AsString
              DeviceId = update.DeviceId.AsString
              SensorId = update.SensorId.AsString
              MeasuredProperty = update.Measurement |> Name
              MeasuredValue = update.Measurement |> Value
              Voltage = (float)update.BatteryVoltage
              SignalStrength = (float)update.SignalStrength
              Timestamp = update.Timestamp }