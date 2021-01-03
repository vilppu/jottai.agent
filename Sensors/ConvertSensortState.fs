namespace Jottai

module internal ConvertSensorState =
    open MongoDB.Bson

    let private ProtocolToStorable (deviceProtocol : DeviceProtocol ) : string =
        match deviceProtocol with
        | ZWavePlus -> "Z-Wave Plus"
        | _ -> ""

    let FromSensorStateUpdate (update : SensorStateUpdate) (previousState : SensorStateStorage.StorableSensorState) : SensorState =
            let previousState =
                if previousState :> obj |> isNull
                then
                    let defaultName = update.DeviceId.AsString + "." + (update.Measurement |> Measurement.Name)
                    SensorStateStorage.InitialState defaultName
                else previousState

            let hasChanged = (update.Measurement |> Measurement.Value) <> previousState.PropertyValue
            let lastActive = update.Timestamp
            let lastUpdated =
                if hasChanged
                then lastActive
                else previousState.LastUpdated

            { DeviceGroupId = update.DeviceGroupId
              GatewayId = update.GatewayId
              PropertyId = update.PropertyId              
              DeviceId = update.DeviceId
              PropertyName = PropertyName previousState.PropertyName
              PropertyDescription = PropertyDescription previousState.PropertyDescription
              Measurement = update.Measurement
              Protocol = update.Protocol
              BatteryVoltage = update.BatteryVoltage
              SignalStrength = update.SignalStrength
              LastUpdated = lastUpdated
              LastActive = lastActive }

    let ToStorable (sensorState : SensorState)
        : SensorStateStorage.StorableSensorState =
        { Id = ObjectId.Empty
          DeviceGroupId = sensorState.DeviceGroupId.AsString
          GatewayId = sensorState.GatewayId.AsString
          DeviceId = sensorState.DeviceId.AsString
          PropertyId = sensorState.PropertyId.AsString
          PropertyName = sensorState.PropertyName.AsString
          PropertyDescription = sensorState.PropertyDescription.AsString
          PropertyType = sensorState.Measurement |> Measurement.Name
          PropertyValue = sensorState.Measurement |> Measurement.Value
          Protocol = sensorState.Protocol |> ProtocolToStorable
          BatteryVoltage = (float)sensorState.BatteryVoltage
          SignalStrength = (float)sensorState.SignalStrength
          LastUpdated = sensorState.LastUpdated
          LastActive = sensorState.LastActive
        }