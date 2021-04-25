namespace Jottai

module internal ConvertDeviceData =
    
    let private ToDeviceDataUpdate
        (deviceGroupId : DeviceGroupId)
        (deviceData : ApiObjects.DeviceData)
        (deviceDatum : ApiObjects.DeviceDatum)
        : (DeviceDataUpdate option) =
        let sensorStateUpdate = ConvertSensorStateUpdate.FromDeviceData
        let devicePropertyUpdate = ConvertDevicePropertyUpdate.FromDeviceData

        match deviceDatum.propertyType with
        | ApiObjects.PropertyType.Voltage -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.Rssi -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.Temperature -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.RelativeHumidity -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.PresenceOfWater -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.Contact -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.Motion -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.Luminance -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.SeismicIntensity -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.Acceleration -> sensorStateUpdate deviceGroupId deviceData deviceDatum
        | ApiObjects.PropertyType.TwoWaySwitch -> devicePropertyUpdate deviceGroupId deviceData deviceDatum
        | _ -> failwithf "%s is not a valid property type" (deviceDatum.propertyType.ToString())        
    
    let ToDeviceDataUpdates (deviceGroupId : DeviceGroupId) (deviceData : ApiObjects.DeviceData)
        : DeviceDataUpdate list =
        let data = 
            match deviceData.data :> obj with
            | null -> list.Empty
            | _ -> deviceData.data

        data
        |> Seq.toList
        |> List.map (fun deviceDatum -> ToDeviceDataUpdate deviceGroupId deviceData deviceDatum)
        |> List.choose (id)
