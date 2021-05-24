namespace Jottai

module internal ConvertDeviceProperty =

    let private ProtocolToApiObject (deviceProtocol : DeviceProtocol ) : string =
        match deviceProtocol with
        | ZWave -> "Z-Wave"
        | ZWavePlus -> "Z-Wave Plus"
        | NotSpecified -> ""
              
    let ToApiObjects (states : DevicePropertyState list) : ApiObjects.DeviceProperty list = 
              states
              |> List.map (fun state ->
                  let (DeviceGroupId deviceGroupId) = state.DeviceGroupId
                  let (GatewayId gatewayId) = state.GatewayId
                  let (DeviceId deviceId) = state.DeviceId
                  let (PropertyId propertyId) = state.PropertyId
                  let (PropertyName propertyName) = state.PropertyName
                  let (PropertyDescription propertyDescription) = state.PropertyDescription
                  { DeviceGroupId = deviceGroupId
                    GatewayId = gatewayId
                    DeviceId = deviceId
                    PropertyId = propertyId
                    PropertyName = propertyName
                    PropertyDescription = propertyDescription                    
                    PropertyType = state.PropertyValue |> DeviceProperty.Name
                    PropertyValue = state.PropertyValue |> DeviceProperty.Value
                    Protocol = state.Protocol |> ProtocolToApiObject
                    LastUpdated = state.LastUpdated
                    LastActive = state.LastActive })