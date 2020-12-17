namespace Jottai

module internal ConvertDeviceProperty =

    let private ProtocolToStorable (deviceProtocol : DeviceProtocol ) : string =
        match deviceProtocol with
        | ZWavePlus -> "Z-Wave Plus"

    let private CommandTypeToStorable (commandType : PropertyType ) : string =
        match commandType with
        | BinarySwitch -> "BinarySwitch"

    let private ProtocolFromStorable (storableProtocol : string ) : DeviceProtocol option =
        match storableProtocol with
        | "Z-Wave Plus" -> ZWavePlus |> Some
        | _ -> None

    let private CommandTypeFromStorable (storableCommandType : string ) : PropertyType option =
        match storableCommandType with
        | "BinarySwitch" -> BinarySwitch |> Some
        | _ -> None
    
    let private FromStorable (storable : DevicePropertyStorage.StorableDeviceProperty) : DeviceProperty option =
        let protocol = storable.Protocol |> ProtocolFromStorable
        let commandType = storable.PropertyType |> CommandTypeFromStorable
        let propertyValue = DeviceProperty.From storable.PropertyType storable.PropertyValue

        match (protocol, commandType, propertyValue) with
        | (Some protocol, Some commandType, Some propertyValue) ->
            let command : DeviceProperty =
                { DeviceGroupId = DeviceGroupId storable.DeviceGroupId
                  GatewayId = GatewayId storable.GatewayId
                  DeviceId = DeviceId storable.DeviceId
                  PropertyId = PropertyId storable.PropertyId
                  PropertyType = commandType                                    
                  PropertyName = PropertyName storable.PropertyName
                  PropertyDescription = PropertyDescription storable.PropertyDescription
                  PropertyValue = propertyValue
                  Protocol = protocol
                  LastUpdated = storable.LastUpdated
                  LastActive = storable.LastActive }
            command |> Some
        | _ -> None
    
    let FromStorables (storables : seq<DevicePropertyStorage.StorableDeviceProperty>) : DeviceProperty list =        
        storables
        |> Seq.map FromStorable
        |> Seq.choose id
        |> Seq.toList
              
    let ToApiObjects (commands : DeviceProperty list) : ApiObjects.DevicePropertyState list = 
              commands
              |> List.map (fun command ->            
                  { DeviceGroupId = command.DeviceGroupId.AsString
                    GatewayId = command.GatewayId.AsString
                    DeviceId = command.DeviceId.AsString
                    PropertyId = command.PropertyId.AsString
                    PropertyType = command.PropertyType |> CommandTypeToStorable
                    PropertyName = command.PropertyName.AsString
                    PropertyDescription = command.PropertyDescription.AsString                    
                    PropertyValue = command.PropertyValue |> DeviceProperty.Value
                    Protocol = command.Protocol |> ProtocolToStorable
                    LastUpdated = command.LastUpdated
                    LastActive = command.LastActive })