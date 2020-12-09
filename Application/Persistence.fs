namespace Jottai

module internal Persistence =

    let Store (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SubscribedToPushNotifications _ -> ()
            | Event.SensorStateChanged sensorStateChanged ->
                if false then
                    let sensorStateUpdate = sensorStateChanged |> Event.ToSensorStateUpdate
                    do! Action.StoreSensorStateChangedEvent sensorStateUpdate
            | Event.SensorNameChanged _ -> ()
        }