namespace Jottai

module DeviceProperty =
    
    type BinarySwitch = 
        | On
        | Off
    
    type DeviceProperty = 
        | BinarySwitch of BinarySwitch

    let From (propertyName : string) (propertyValue : obj) : DeviceProperty option =
        match propertyName with
        | "BinarySwitch" ->
            if (propertyValue :?> bool)
            then BinarySwitch On |> Some
            else BinarySwitch Off |> Some
        | _ -> None

    let FromString (propertyName : string) (propertyValue : string) : DeviceProperty option =        
        match propertyName with
        | "BinarySwitch" ->
            let valueIsBoolean, isOn = System.Boolean.TryParse(propertyValue)
            match (valueIsBoolean, isOn) with
            | (true, propertyValue) ->            
                if propertyValue
                then BinarySwitch On |> Some
                else BinarySwitch Off |> Some
            | _ -> None
        | _ -> None

    let ValueAsString (deviceProperty : DeviceProperty) : string =
        match deviceProperty with
        | BinarySwitch binarySwitch ->
            match binarySwitch with
            | On -> "True"
            | Off -> "False"

    let Value (deviceProperty : DeviceProperty) : obj =
        match deviceProperty with
        | BinarySwitch binarySwitch ->
            match binarySwitch with
            | On -> true :> obj
            | Off -> false :> obj

    let Name (deviceProperty : DeviceProperty) : string =
        match Reflection.FSharpValue.GetUnionFields(deviceProperty, deviceProperty.GetType()) with
        | unionCaseInfo, _ -> unionCaseInfo.Name
 