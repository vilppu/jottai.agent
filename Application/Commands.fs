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
    
    let private SensorStateUpdateToCommand (sensorStateUpdate : SensorStateUpdate) : Command.Command =
        sensorStateUpdate
        |> ChangeSensorState
        |> Command.ChangeSensorState
    
    let private DevicePropertyUpdateToCommand (devicePropertyUpdate: DevicePropertyUpdate) : Command.Command =
        devicePropertyUpdate
        |> ChangeDeviceProperty
        |> Command.ChangeDevicePropertyState
    
    let private DeviceDataUpdateToCommand (deviceDataUpdate : DeviceDataUpdate) : Command.Command =
        match deviceDataUpdate with
        | SensorStateUpdate sensorStateUpdate -> sensorStateUpdate |> SensorStateUpdateToCommand
        | DevicePropertyUpdate devicePropertyUpdate -> devicePropertyUpdate |> DevicePropertyUpdateToCommand
    
    let private DeviceDataUpdatesToCommands (deviceDataUpdates : DeviceDataUpdate list) : Command.Command list =
        deviceDataUpdates
        |> List.map DeviceDataUpdateToCommand

    let FromDeviceData deviceGroupId (deviceData : ApiObjects.DeviceData) : Command.Command list =
        deviceData
        |> ConvertDeviceData.ToDeviceDataUpdates (DeviceGroupId deviceGroupId)
        |> DeviceDataUpdatesToCommands
