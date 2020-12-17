namespace Jottai

module internal Action =

    let StoreDeviceProperty (deviceProperty : DeviceProperty) : Async<unit> =
        async {
            let storable = ConvertDeviceProperty.ToStorable deviceProperty                
            do! DevicePropertyStorage.StoreDeviceProperty storable
        }
