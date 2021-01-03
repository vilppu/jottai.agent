namespace Jottai

module ConvertDevicePropertyUpdate =

    let private ParseTimestamp timestamp =
        if System.String.IsNullOrWhiteSpace(timestamp)
        then System.DateTimeOffset.UtcNow
        else System.DateTimeOffset.Parse(timestamp)

    let private ParseTwoWaySwitch (datum : ApiObjects.DeviceDatum) : DeviceProperty.DeviceProperty option =
        let valueIsBoolean, isOn = System.Boolean.TryParse(datum.value)
        match valueIsBoolean, isOn with
        | true, true -> DeviceProperty.TwoWaySwitch DeviceProperty.On |> Some
        | true, false -> DeviceProperty.TwoWaySwitch DeviceProperty.Off |> Some
        | _ -> None

    let private ParsePropertyValue (datum : ApiObjects.DeviceDatum)
        : DeviceProperty.DeviceProperty option =                
        match datum.propertyType |> Convert.PropertyTypeFromApiObject with
        | PropertyType.TwoWaySwitch -> datum |> ParseTwoWaySwitch
        | _ -> None

    let FromDeviceData
        (deviceGroupId : DeviceGroupId)
        (deviceData : ApiObjects.DeviceData)
        (datum : ApiObjects.DeviceDatum)
        : DeviceDataUpdate option =
        
        match datum |> ParsePropertyValue with
        | Some propertyValue ->
            { DeviceGroupId = deviceGroupId
              GatewayId = GatewayId deviceData.gatewayId
              DeviceId = DeviceId deviceData.deviceId
              PropertyId = PropertyId datum.propertyId              
              PropertyName = PropertyName datum.propertyName
              PropertyDescription = PropertyDescription datum.propertyDescription
              PropertyValue = propertyValue
              Protocol = deviceData.protocol |> Convert.ProtocolFromApiObject
              Timestamp = deviceData.timestamp |> ParseTimestamp }
            |> DevicePropertyUpdate
            |> Some
        | _ -> None
