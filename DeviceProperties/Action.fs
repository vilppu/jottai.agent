namespace Jottai

module internal Action =

    let GetDevicePropertyState (update : DevicePropertyUpdate) : Async<DevicePropertyState> =
        async {
            let! previousState = DevicePropertyStorage.GetDeviceProperty update.DeviceGroupId.AsString update.GatewayId.AsString update.DeviceId.AsString update.PropertyId.AsString 
            return ConvertDevicePropertyState.FromDevicePropertyUpdate update previousState
        }

    let StoreDeviceProperty (deviceProperty : DevicePropertyState) : Async<unit> =
        async {
            let! previous = DevicePropertyStorage.GetDeviceProperty deviceProperty.DeviceGroupId.AsString deviceProperty.GatewayId.AsString deviceProperty.DeviceId.AsString deviceProperty.PropertyId.AsString 
            
            let name =
                match previous with
                | Some previous -> previous.PropertyName
                | None -> deviceProperty.PropertyName.AsString

            let storable = { ConvertDevicePropertyState.ToStorable deviceProperty
                             with PropertyName = name }
            do! DevicePropertyStorage.StoreDeviceProperty storable
        }
