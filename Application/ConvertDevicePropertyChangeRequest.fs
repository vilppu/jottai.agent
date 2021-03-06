﻿namespace Jottai

module internal ConvertDevicePropertyChangeRequest =

    let ToApiObject (devicePropertyChangeRequest : Event.DevicePropertyChangeRequest) : ApiObjects.DevicePropertyChangeRequest =
        { GatewayId = devicePropertyChangeRequest.GatewayId.AsString
          DeviceId = devicePropertyChangeRequest.DeviceId.AsString
          PropertyId = devicePropertyChangeRequest.PropertyId.AsString
          PropertyValue = devicePropertyChangeRequest.PropertyValue |> DeviceProperty.ValueAsString }
