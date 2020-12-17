namespace Jottai

module internal Commands =

    let private ChangeSensorState (sensorStateUpdate : SensorStateUpdate) : Command.ChangeSensorState =
        {
            SensorStateUpdate = sensorStateUpdate
        }

    let private ChangeDeviceProperty (deviceProperty : DeviceProperty) : Command.SetDevicePropertyAvailable =
        {
            DeviceProperty = deviceProperty
        }
    
    let private FromSensorStateUpdates (sensorStateUpdates : SensorStateUpdate list) : Command.Command list =
        sensorStateUpdates
        |> List.map (fun sensorStateUpdate -> ChangeSensorState sensorStateUpdate)
        |> List.map (fun changeSensorState -> Command.ChangeSensorState changeSensorState)
    
    let private FromDeviceProperties (deviceProperties : DeviceProperty list) : Command.Command list =
        deviceProperties
        |> List.map (fun deviceProperty -> ChangeDeviceProperty deviceProperty)
        |> List.map (fun changeDeviceProperty -> Command.SetDevicePropertyAvailable changeDeviceProperty)

    let FromDeviceData deviceGroupId (deviceData : ApiObjects.DeviceData) : Command.Command list =

        let sensorStateUpdates =
            deviceData
            |> ConvertDeviceData.ToSensorStateUpdates (DeviceGroupId deviceGroupId)
            |> FromSensorStateUpdates
            
        let deviceProperties =
            deviceData
            |> ConvertDeviceData.ToDeviceProperties (DeviceGroupId deviceGroupId)
            |> FromDeviceProperties
            
        sensorStateUpdates
        |> List.append deviceProperties
