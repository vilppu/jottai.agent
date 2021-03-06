namespace Jottai

module internal ConvertDevicePropertyState =
    open MongoDB.Bson

    let private ProtocolToStorable (deviceProtocol : DeviceProtocol ) : string =
        match deviceProtocol with
        | ZWavePlus -> "Z-Wave Plus"
        | _ -> ""

    let FromDevicePropertyUpdate (update : DevicePropertyStateUpdate) (previousState : DevicePropertyStorage.StorableDeviceProperty option) : DevicePropertyState =
        let previousState =
            match previousState with
            | Some previousState -> previousState
            | None -> DevicePropertyStorage.InitialState update.PropertyName

        let hasChanged = (update.PropertyValue |> DeviceProperty.Value) <> previousState.PropertyValue
        let lastActive = update.Timestamp
        let lastUpdated =
            if hasChanged
            then lastActive
            else previousState.LastUpdated

        { DeviceGroupId = update.DeviceGroupId
          GatewayId = update.GatewayId
          DeviceId = update.DeviceId
          PropertyId = update.PropertyId
          PropertyName = update.PropertyName
          PropertyDescription = update.PropertyDescription
          PropertyValue = update.PropertyValue
          Protocol = update.Protocol
          LastUpdated = lastUpdated 
          LastActive = lastActive }

    let ToStorable (deviceProperty : DevicePropertyState)
        : DevicePropertyStorage.StorableDeviceProperty =
        { Id = ObjectId.Empty
          DeviceGroupId = deviceProperty.DeviceGroupId.AsString
          GatewayId = deviceProperty.GatewayId.AsString
          DeviceId = deviceProperty.DeviceId.AsString
          PropertyId = deviceProperty.PropertyId.AsString
          PropertyName = deviceProperty.PropertyName.AsString
          PropertyDescription = deviceProperty.PropertyDescription.AsString
          PropertyType = deviceProperty.PropertyValue |> DeviceProperty.Name
          PropertyValue = deviceProperty.PropertyValue |> DeviceProperty.Value
          Protocol = deviceProperty.Protocol |> ProtocolToStorable
          LastUpdated = deviceProperty.LastUpdated
          LastActive = deviceProperty.LastActive
        }