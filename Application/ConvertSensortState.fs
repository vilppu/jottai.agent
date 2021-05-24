namespace Jottai

module internal ConvertSensorState =

    let private ProtocolToApiObject (deviceProtocol : DeviceProtocol ) : string =
        match deviceProtocol with
        | ZWave -> "Z-Wave"
        | ZWavePlus -> "Z-Wave Plus"
        | NotSpecified -> ""
              
    let ToApiObjects (statuses : SensorState list) : ApiObjects.SensorState list = 
              statuses
              |> List.map (fun state ->
                  let (DeviceGroupId deviceGroupId) = state.DeviceGroupId
                  let (DeviceId deviceId) = state.DeviceId
                  let (PropertyId propertyId) = state.PropertyId
                  let (PropertyName propertyName) = state.PropertyName
                  { DeviceGroupId = deviceGroupId
                    DeviceId = deviceId
                    PropertyId = propertyId
                    PropertyName = propertyName
                    MeasuredProperty = state.Measurement |> Measurement.Name
                    MeasuredValue = state.Measurement |> Measurement.Value
                    Protocol = state.Protocol |> ProtocolToApiObject
                    BatteryVoltage = float(state.BatteryVoltage)
                    SignalStrength = state.SignalStrength
                    LastUpdated = state.LastUpdated
                    LastActive = state.LastActive })
