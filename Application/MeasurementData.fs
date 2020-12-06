namespace Jottai

[<AutoOpen>]
module internal MeasurementData =
    
    open Microsoft.FSharp.Reflection

    let Value (measurement : Measurement.Measurement) : obj =
        match measurement with
        | Measurement.Voltage voltage ->
            float(voltage) :> obj

        | Measurement.Rssi rssi ->
            float(rssi) :> obj

        | Measurement.Temperature temperature ->
            float(temperature) :> obj

        | Measurement.RelativeHumidity relativeHumidity ->
            float(relativeHumidity) :> obj

        | Measurement.PresenceOfWater presenceOfWater ->
            match presenceOfWater with
            | Measurement.NotPresent -> false :> obj
            | Measurement.Present -> true :> obj

        | Measurement.Contact contact ->
            match contact with
            | Measurement.Open -> false :> obj
            | Measurement.Closed -> true :> obj

        | Measurement.Measurement.Motion motion -> 
            match motion with
            | Measurement.NoMotion -> false :> obj
            | Measurement.Motion -> true :> obj

        | Measurement.Unsupported _ -> 0 :> obj

    let Name (measurement : Measurement.Measurement) : string =
        match FSharpValue.GetUnionFields(measurement, measurement.GetType()) with
        | unionCaseInfo, _ -> unionCaseInfo.Name
 