namespace Jottai

module internal Convert =

    let PropertyTypeFromApiObject (propertyType : ApiObjects.PropertyType) : PropertyType.PropertyType =
        match propertyType with
        | ApiObjects.PropertyType.TwoWaySwitch -> PropertyType.TwoWaySwitch
        | ApiObjects.PropertyType.Sensor -> PropertyType.Sensor
        | _ -> PropertyType.Sensor


    let ProtocolFromApiObject (protocol : ApiObjects.Protocol) : DeviceProtocol =
        match protocol with
        | ApiObjects.Protocol.ZWave -> ZWave
        | ApiObjects.Protocol.ZWavePlus -> ZWavePlus
        | _ -> NotSpecified
