namespace Jottai

module internal ConvertDeviceProperty =

    let private ProtocolToApiObject (deviceProtocol : DeviceProtocol ) : string =
        match deviceProtocol with
        | ZWave -> "Z-Wave"
        | ZWavePlus -> "Z-Wave Plus"
        | NotSpecified -> ""

    let private ProtocolFromStorable (storableProtocol : string ) : DeviceProtocol =
        match storableProtocol with
        | "Z-Wave" -> ZWave
        | "Z-Wave Plus" -> ZWavePlus
        | _ -> NotSpecified
    
    let private FromStorable (storable : DevicePropertyStorage.StorableDeviceProperty) : DevicePropertyState option =        
        let propertyValue = DeviceProperty.From storable.PropertyType storable.PropertyValue

        match propertyValue with
        | Some propertyValue ->
            let command : DevicePropertyState =
                { DeviceGroupId = DeviceGroupId storable.DeviceGroupId
                  GatewayId = GatewayId storable.GatewayId
                  DeviceId = DeviceId storable.DeviceId
                  PropertyId = PropertyId storable.PropertyId                  
                  PropertyName = PropertyName storable.PropertyName
                  PropertyDescription = PropertyDescription storable.PropertyDescription
                  PropertyValue = propertyValue
                  Protocol = storable.Protocol |> ProtocolFromStorable
                  LastUpdated = storable.LastUpdated
                  LastActive = storable.LastActive }
            command |> Some
        | _ -> None
    
    let FromStorables (storables : seq<DevicePropertyStorage.StorableDeviceProperty>) : DevicePropertyState list =        
        storables
        |> Seq.map FromStorable
        |> Seq.choose id
        |> Seq.toList
              
    let ToApiObjects (states : DevicePropertyState list) : ApiObjects.DeviceProperty list = 
              states
              |> List.map (fun state ->            
                  { DeviceGroupId = state.DeviceGroupId.AsString
                    GatewayId = state.GatewayId.AsString
                    DeviceId = state.DeviceId.AsString
                    PropertyId = state.PropertyId.AsString
                    PropertyName = state.PropertyName.AsString
                    PropertyDescription = state.PropertyDescription.AsString                    
                    PropertyType = state.PropertyValue |> DeviceProperty.Name
                    PropertyValue = state.PropertyValue |> DeviceProperty.Value
                    Protocol = state.Protocol |> ProtocolToApiObject
                    LastUpdated = state.LastUpdated
                    LastActive = state.LastActive })