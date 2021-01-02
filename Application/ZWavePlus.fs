namespace Jottai

module ZWavePlus =
    let private ToCommandType (propertyTypeId : string) : PropertyType option =
        let isNumeric, numericCommandType = System.Int32.TryParse propertyTypeId
        if isNumeric then
            match numericCommandType with
            | 0x25 -> BinarySwitch |> Some
            | _ -> None
        else
            None

    let ToDeviceDataUpdate
        (deviceGroupId : DeviceGroupId)
        (deviceData : ApiObjects.DeviceData)
        (datum : ApiObjects.DeviceDatum)
        : DeviceDataUpdate option =
    
        let timestamp =
            if System.String.IsNullOrWhiteSpace(deviceData.timestamp)
            then System.DateTimeOffset.UtcNow
            else System.DateTimeOffset.Parse(deviceData.timestamp)

        let commandType = datum.propertyTypeId |> ToCommandType
        let valueIsBoolean, isOn = System.Boolean.TryParse(datum.value)

        match (commandType, valueIsBoolean) with
            | (Some commandType, true) ->
                let propertyValue =
                  if isOn
                  then DeviceProperty.BinarySwitch DeviceProperty.On
                  else DeviceProperty.BinarySwitch DeviceProperty.Off
                let devicePropertyUpdate : DevicePropertyUpdate =
                    { DeviceGroupId = deviceGroupId
                      GatewayId = GatewayId deviceData.gatewayId
                      DeviceId = DeviceId deviceData.deviceId
                      PropertyId = PropertyId datum.propertyId
                      PropertyType = commandType
                      PropertyName = PropertyName datum.propertyName
                      PropertyDescription = PropertyDescription datum.propertyDescription
                      PropertyValue = propertyValue
                      Protocol = ZWavePlus
                      Timestamp = timestamp }                
                devicePropertyUpdate |> DevicePropertyUpdate |> Some
            | _ -> None
