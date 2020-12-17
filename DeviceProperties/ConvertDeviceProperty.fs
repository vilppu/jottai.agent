namespace Jottai

module internal ConvertDeviceProperty =
    open MongoDB.Bson

    let private ProtocolToStorable (deviceProtocol : DeviceProtocol ) : string =
        match deviceProtocol with
        | ZWavePlus -> "Z-Wave Plus"

    let private CommandTypeToStorable (commandType : PropertyType ) : string =
        match commandType with
        | BinarySwitch -> "BinarySwitch"

    let ToStorable (deviceProperty : DeviceProperty)
        : DevicePropertyStorage.StorableDeviceProperty =
        { Id = ObjectId.Empty
          DeviceGroupId = deviceProperty.DeviceGroupId.AsString
          GatewayId = deviceProperty.GatewayId.AsString
          DeviceId = deviceProperty.DeviceId.AsString
          PropertyId = deviceProperty.PropertyId.AsString
          PropertyType = deviceProperty.PropertyType |> CommandTypeToStorable
          PropertyName = deviceProperty.PropertyName.AsString
          PropertyDescription = deviceProperty.PropertyDescription.AsString
          PropertyValue = deviceProperty.PropertyValue |> DeviceProperty.Value          
          Protocol = deviceProperty.Protocol |> ProtocolToStorable
          LastUpdated = deviceProperty.LastUpdated
          LastActive = deviceProperty.LastActive
        }