namespace Jottai

module internal Convert =

    //let PropertyTypeFromApiObject (propertyType : ApiObjects.PropertyType) : PropertyType.PropertyType =
    //    match propertyType with
    //    | ApiObjects.PropertyType.Voltage -> PropertyType.Voltage
    //    | ApiObjects.PropertyType.Rssi -> PropertyType.Rssi
    //    | ApiObjects.PropertyType.Temperature -> PropertyType.Temperature
    //    | ApiObjects.PropertyType.RelativeHumidity -> PropertyType.RelativeHumidity
    //    | ApiObjects.PropertyType.PresenceOfWater -> PropertyType.PresenceOfWater
    //    | ApiObjects.PropertyType.Contact -> PropertyType.Contact
    //    | ApiObjects.PropertyType.Motion -> PropertyType.Motion
    //    | ApiObjects.PropertyType.TwoWaySwitch -> PropertyType.TwoWaySwitch
    //    | _ -> failwithf "%s is not a valid property type" (propertyType.ToString())


    let ProtocolFromApiObject (protocol : ApiObjects.Protocol) : DeviceProtocol =
        match protocol with
        | ApiObjects.Protocol.NotSpecified -> NotSpecified
        | ApiObjects.Protocol.ZWave -> ZWave
        | ApiObjects.Protocol.ZWavePlus -> ZWavePlus
        | _ -> failwithf "%s is not a valid protocol type" (protocol.ToString())
