namespace Jottai

[<AutoOpen>]
module DevicePropertyToDeviceDataMapping =

    let private ToDeviceDatum (deviceProperty : DeviceProperty.DeviceProperty)
        : ApiObjects.DeviceDatum =
        match deviceProperty with
        | DeviceProperty.TwoWaySwitch _ ->
            { propertyId = "0x1234567890"
              propertyName = "Switch"
              propertyDescription = "Turn On/Off Device"              
              propertyType = ApiObjects.PropertyType.TwoWaySwitch
              unitOfMeasurement = ""
              value = "True"
              valueType = "bool"
              formattedValue = "True"
              minimumValue = "0"
              maximumValue = "0" }
    
    let WithDevicePropertyCommand (deviceProperty : DeviceProperty.DeviceProperty) deviceData : ApiObjects.DeviceData =
        { deviceData 
          with data = [ deviceProperty |> ToDeviceDatum ] }
