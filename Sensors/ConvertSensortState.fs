namespace Jottai

[<AutoOpen>]
module internal ConvertStorableSensorState =
    open MongoDB.Bson

    let FromSensorStateUpdate (update : SensorStateUpdate) (previousState : SensorStateStorage.StorableSensorState) : SensorState =
            let previousState =
                if previousState :> obj |> isNull
                then
                    let defaultName = update.DeviceId.AsString + "." + (update.Measurement |> Name)
                    SensorStateStorage.InitialState defaultName
                else previousState

            let hasChanged = (update.Measurement |> Value) <> previousState.MeasuredValue
            let lastActive = update.Timestamp
            let lastUpdated =
                if hasChanged
                then lastActive
                else previousState.LastUpdated

            { SensorId = update.SensorId
              DeviceGroupId = update.DeviceGroupId
              DeviceId = update.DeviceId
              SensorName = previousState.SensorName
              Measurement = update.Measurement
              BatteryVoltage = update.BatteryVoltage
              SignalStrength = update.SignalStrength
              LastUpdated = lastUpdated
              LastActive = lastActive }

    let ToStorable (sensorState : SensorState)
        : SensorStateStorage.StorableSensorState =
        { Id = ObjectId.Empty
          DeviceGroupId = sensorState.DeviceGroupId.AsString
          DeviceId = sensorState.DeviceId.AsString
          SensorId = sensorState.SensorId.AsString
          SensorName = sensorState.SensorName
          MeasuredProperty = sensorState.Measurement |> Name
          MeasuredValue = sensorState.Measurement |> Value
          BatteryVoltage = (float)sensorState.BatteryVoltage
          SignalStrength = (float)sensorState.SignalStrength
          LastUpdated = sensorState.LastUpdated
          LastActive = sensorState.LastActive
        }