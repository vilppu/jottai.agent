namespace Jottai

module internal ConvertDevicePropertyChangeRequest =

    let ToApiObject (devicePropertyChangeRequest : Event.DevicePropertyChangeRequest) : ApiObjects.DevicePropertyChangeRequest =            
        let (GatewayId gatewayId) = devicePropertyChangeRequest.GatewayId
        let (DeviceId deviceId) = devicePropertyChangeRequest.DeviceId
        let (PropertyId propertyId) = devicePropertyChangeRequest.PropertyId
        { GatewayId = gatewayId
          DeviceId = deviceId
          PropertyId = propertyId
          PropertyValue = devicePropertyChangeRequest.PropertyValue |> DeviceProperty.ValueAsString }
