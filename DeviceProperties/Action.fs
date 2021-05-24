namespace Jottai

module internal Action =

    let GetDevicePropertyState (update : DevicePropertyStateUpdate) : Async<DevicePropertyState> =
        async {
            let! devicePropertyState = DevicePropertyStorage.GetDeviceProperty update
            return devicePropertyState
        }

    let StoreDeviceProperty (deviceProperty : DevicePropertyState) : Async<unit> =
        async {            
            do! DevicePropertyStorage.StoreDeviceProperty deviceProperty
        }
