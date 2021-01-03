namespace Jottai

module internal ConvertDeviceData =
    
    let private ToDeviceDataUpdate
        (deviceGroupId : DeviceGroupId)
        (deviceData : ApiObjects.DeviceData)
        (deviceDatum : ApiObjects.DeviceDatum)
        : (DeviceDataUpdate option) =

        match deviceDatum.propertyType |> Convert.PropertyTypeFromApiObject with
        | PropertyType.Sensor -> ConvertSensorStateUpdate.FromDeviceData deviceGroupId deviceData deviceDatum
        | _ -> ConvertDevicePropertyUpdate.FromDeviceData deviceGroupId deviceData deviceDatum
    
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
