namespace Jottai

module internal Action =

    let GetSensorState (update : SensorStateUpdate) : Async<SensorState> =
        async {
            let! sensorState = SensorStateStorage.GetSensorState update
            return sensorState
        }

    let GetSensorHistory (update : SensorStateUpdate) : Async<SensorHistory> =
        async {
            let! sensorHistory = SensorHistoryStorage.GetSensorHistory update.DeviceGroupId update.PropertyId
            return sensorHistory
        }    

    let StoreSensorState (sensorState : SensorState) : Async<unit> =
        async {            
            do! SensorStateStorage.StoreSensorState sensorState
        }

    let StoreSensorHistory (sensorState : SensorState) (sensorHistory : SensorHistory) : Async<unit> =
        async {
            let hasChanged = sensorState.LastUpdated = sensorState.LastActive
            if hasChanged then
                do! SensorHistoryStorage.UpsertSensorHistory sensorState sensorHistory
        }