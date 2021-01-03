namespace Jottai

module DeviceProperty =
    
    type TwoWaySwitch = 
        | On
        | Off
    
    type DeviceProperty = 
        | TwoWaySwitch of TwoWaySwitch

    let IsDevicePropert (propertyName : string) (propertyValue : obj) : DeviceProperty option =
        match propertyName with
        | "TwoWaySwitch" ->
            if (propertyValue :?> bool)
            then TwoWaySwitch On |> Some
            else TwoWaySwitch Off |> Some
        | _ -> None

    let From (propertyName : string) (propertyValue : obj) : DeviceProperty option =
        match propertyName with
        | "TwoWaySwitch" ->
            if (propertyValue :?> bool)
            then TwoWaySwitch On |> Some
            else TwoWaySwitch Off |> Some
        | _ -> None

    let FromString (propertyName : string) (propertyValue : string) : DeviceProperty option =        
        match propertyName with
        | "TwoWaySwitch" ->
            let valueIsBoolean, isOn = System.Boolean.TryParse(propertyValue)
            match (valueIsBoolean, isOn) with
            | (true, propertyValue) ->            
                if propertyValue
                then TwoWaySwitch On |> Some
                else TwoWaySwitch Off |> Some
            | _ -> None
        | _ -> None

    let ValueAsString (deviceProperty : DeviceProperty) : string =
        match deviceProperty with
        | TwoWaySwitch twoWaySwitch ->
            match twoWaySwitch with
            | On -> "True"
            | Off -> "False"

    let Value (deviceProperty : DeviceProperty) : obj =
        match deviceProperty with
        | TwoWaySwitch binarySwitch ->
            match binarySwitch with
            | On -> true :> obj
            | Off -> false :> obj

    let Name (deviceProperty : DeviceProperty) : string =
        match Reflection.FSharpValue.GetUnionFields(deviceProperty, deviceProperty.GetType()) with
        | unionCaseInfo, _ -> unionCaseInfo.Name
 