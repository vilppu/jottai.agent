namespace Jottai

module private Action =
    let SendNotifications httpSend (sensorState : SensorState) : Async<unit> =
        async {
            match sensorState.Measurement with
            | Measurement.Measurement.Contact _ ->
                if sensorState.LastUpdated = sensorState.LastActive then
                    do! Firebase.Send httpSend sensorState
            | Measurement.Measurement.PresenceOfWater _ ->                
                if sensorState.LastUpdated = sensorState.LastActive then
                    do! Firebase.Send httpSend sensorState
            | Measurement.Measurement.Motion _ ->                
                if sensorState.LastUpdated = sensorState.LastActive then
                    do! Firebase.Send httpSend sensorState
            | _ -> ()
        }
   