namespace Jottai

module internal Commands =

    let private ChangeSensorState (sensorStateUpdate : SensorStateUpdate) : Command.ChangeSensorState =
        {
            SensorStateUpdate = sensorStateUpdate
        }

    let private ChangeDeviceProperty (deviceProperty : DevicePropertyUpdate) : Command.ChangeDevicePropertyState =
        {
            DeviceProperty = deviceProperty
        }
    
    let private FromSensorStateUpdates (sensorStateUpdates : SensorStateUpdate list) : Command.Command list =
        sensorStateUpdates
        |> List.map (fun sensorStateUpdate -> ChangeSensorState sensorStateUpdate)
        |> List.map (fun changeSensorState -> Command.ChangeSensorState changeSensorState)
    
    let private FromDevicePropertyUpdates (deviceProperties : DevicePropertyUpdate list) : Command.Command list =
        deviceProperties
        |> List.map (fun deviceProperty -> ChangeDeviceProperty deviceProperty)
        |> List.map (fun changeDeviceProperty -> Command.ChangeDevicePropertyState changeDeviceProperty)

    let FromDeviceData deviceGroupId (deviceData : ApiObjects.DeviceData) : Command.Command list =

        let sensorStateUpdates =
            deviceData
            |> ConvertDeviceData.ToSensorStateUpdates (DeviceGroupId deviceGroupId)
            |> FromSensorStateUpdates
            
        let deviceProperties =
            deviceData
            |> ConvertDeviceData.ToDevicePropertyUpdates (DeviceGroupId deviceGroupId)
            |> FromDevicePropertyUpdates
            
        sensorStateUpdates
        |> List.append deviceProperties
