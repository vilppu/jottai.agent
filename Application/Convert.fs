namespace Jottai

module internal Convert =

    let ProtocolFromApiObject (protocol : ApiObjects.Protocol) : DeviceProtocol =
        match protocol with
        | ApiObjects.Protocol.NotSpecified -> NotSpecified
        | ApiObjects.Protocol.ZWave -> ZWave
        | ApiObjects.Protocol.ZWavePlus -> ZWavePlus
        | _ -> failwithf "%s is not a valid protocol type" (protocol.ToString())
