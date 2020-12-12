namespace Jottai

module internal Persistence =

    let Store (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SensorStateChanged sensorStateChanged ->                
                let sensorStateUpdate = sensorStateChanged |> Event.ToSensorStateUpdate                
                let storableSensorEvent = UpdateToStorable sensorStateUpdate
                do! SensorEventStorage.StoreSensorEvent storableSensorEvent                
            | Event.SensorNameChanged _ -> ()
            | _ -> ()
        }
