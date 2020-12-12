namespace Jottai

module internal Action =

    let GetSensorState (update : SensorStateUpdate) : Async<SensorState> =
        async {
            let! previousState = SensorStateStorage.GetSensorState update.DeviceGroupId.AsString update.SensorId.AsString
            return ConvertStorableSensorState.FromSensorStateUpdate update previousState
        }

    let GetSensorHistory (update : SensorStateUpdate) : Async<SensorHistory> =
        async {
            let! sensorHistory = SensorHistoryStorage.GetSensorHistory update.DeviceGroupId.AsString update.SensorId.AsString
            return FromStorable sensorHistory
        }    

    let StoreSensorState (sensorState : SensorState) : Async<unit> =
        async {
            let storable = ToStorable sensorState                
            do! SensorStateStorage.StoreSensorState storable
        }

    let StoreSensorHistory (sensorState : SensorState) (sensorHistory : SensorHistory) : Async<unit> =
        async {
            let hasChanged = sensorState.LastUpdated = sensorState.LastActive
            if hasChanged then
                let storableSensorHistory = ConvertSensorHistory.ToStorable sensorState sensorHistory
                do! SensorHistoryStorage.UpsertSensorHistory storableSensorHistory
        }