namespace Jottai

module SensorEventHandler =

    let Handle (publish : Event.Event->Async<unit>) (event : Event.Event) : Async<unit> =
        async {
            match event with
            | Event.SensorStateChanged event ->
                let sensorStateUpdate = event |> Event.ToSensorStateUpdate
                let! sensorState = Action.GetSensorState sensorStateUpdate
                let! sensorHistory = Action.GetSensorHistory sensorStateUpdate
                do! Action.StoreSensorState sensorState
                do! Action.StoreSensorHistory sensorState sensorHistory
                do! publish (Event.SensorStateStored sensorState)

            | Event.SensorNameChanged event ->
                do! SensorStateStorage.StoreSensorName event.DeviceGroupId event.PropertyId event.PropertyName
                do! publish (Event.SensorNameStored event)

            | _ -> ()
        }