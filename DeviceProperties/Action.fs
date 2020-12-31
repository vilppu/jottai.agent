namespace Jottai

module internal Action =

    let StoreDeviceProperty (deviceProperty : DeviceProperty) : Async<unit> =
        async {
            let! previous = DevicePropertyStorage.GetDeviceProperty deviceProperty.DeviceGroupId.AsString deviceProperty.GatewayId.AsString deviceProperty.DeviceId.AsString deviceProperty.PropertyId.AsString 
            
            let name =
                match previous with
                | Some previous -> previous.PropertyName
                | None -> deviceProperty.PropertyName.AsString

            let storable = { ConvertDeviceProperty.ToStorable deviceProperty
                             with PropertyName = name }
            do! DevicePropertyStorage.StoreDeviceProperty storable
        }
