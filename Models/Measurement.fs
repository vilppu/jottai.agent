namespace Jottai

module Measurement = 

    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols

    type Voltage = float<V>
    
    type Rssi = float
    
    type Temperature = float<C>
    
    type RelativeHumidity = float
    
    type PresenceOfWater = 
        | NotPresent
        | Present
    
    type Contact = 
        | Closed
        | Open
    
    type Motion = 
        | NoMotion
        | Motion
    
    type Measurement = 
        | Voltage of Voltage
        | Rssi of Rssi
        | Temperature of Temperature
        | RelativeHumidity of RelativeHumidity
        | PresenceOfWater of PresenceOfWater
        | Contact of Contact
        | Motion of Motion

    let From (measuredProperty : string) (measuredValue : obj) : Measurement option =
        match measuredProperty with
        | "Voltage" ->
            Voltage ((measuredValue :?> float) * 1.0<V>)
            |> Some
        | "Rssi" ->
            Rssi ((measuredValue :?> float))
            |> Some
        | "Temperature" ->
            Temperature ((measuredValue :?> float) * 1.0<C>)
            |> Some
        | "RelativeHumidity" ->
            RelativeHumidity (measuredValue :?> float)
            |> Some
        | "PresenceOfWater" ->
            if (measuredValue :?> bool)
            then PresenceOfWater Present |> Some
            else PresenceOfWater NotPresent |> Some
        | "Contact" ->
            if (measuredValue :?> bool)
            then Contact Closed |> Some
            else Contact Open |> Some
        | "Motion" ->
            if (measuredValue :?> bool)
            then Motion Motion.Motion |> Some
            else Motion NoMotion |> Some
        | _ -> None
        

    let Value (measurement : Measurement) : obj =
        match measurement with
        | Voltage voltage ->
            float(voltage) :> obj

        | Rssi rssi ->
            float(rssi) :> obj

        | Temperature temperature ->
            float(temperature) :> obj

        | RelativeHumidity relativeHumidity ->
            float(relativeHumidity) :> obj

        | PresenceOfWater presenceOfWater ->
            match presenceOfWater with
            | NotPresent -> false :> obj
            | Present -> true :> obj

        | Contact contact ->
            match contact with
            | Open -> false :> obj
            | Closed -> true :> obj

        | Motion motion -> 
            match motion with
            | NoMotion -> false :> obj
            | Motion.Motion -> true :> obj     

    let Name (measurement : Measurement) : string =
        match Reflection.FSharpValue.GetUnionFields(measurement, measurement.GetType()) with
        | unionCaseInfo, _ -> unionCaseInfo.Name
 