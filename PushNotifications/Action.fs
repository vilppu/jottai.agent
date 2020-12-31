namespace Jottai

module private Action =
    let SendSensorStateNotifications httpSend (sensorState : SensorState) : Async<unit> =
        async {
            match sensorState.Measurement with
            | Measurement.Measurement.Contact _ ->
                if sensorState.LastUpdated = sensorState.LastActive then
                    do! Firebase.SendSensorStateNotification httpSend sensorState
            | Measurement.Measurement.PresenceOfWater _ ->                
                if sensorState.LastUpdated = sensorState.LastActive then
                    do! Firebase.SendSensorStateNotification httpSend sensorState
            | Measurement.Measurement.Motion _ ->                
                if sensorState.LastUpdated = sensorState.LastActive then
                    do! Firebase.SendSensorStateNotification httpSend sensorState
            | _ -> ()
        }
   
    let SendDevicePropertyStateNotifications httpSend (devicePropertyState : DevicePropertyState) : Async<unit> =
        async {
            match devicePropertyState.PropertyValue with
            | DeviceProperty.BinarySwitch _ ->
                if devicePropertyState.LastUpdated = devicePropertyState.LastActive then
                    do! Firebase.SendDevicePropertyStateNotification httpSend devicePropertyState
        }
   