namespace Jottai

module internal Action =

    let StoreSensorStateChangedEvent (update : SensorStateUpdate) : Async<unit> =
        async {
            let storableSensorEvent = UpdateToStorable update
            do! SensorEventStorage.StoreSensorEvent storableSensorEvent
        }
