namespace Jottai

module internal Command =
   
    type SubscribeToPushNotifications =
        { DeviceGroupId : DeviceGroupId
          Subscription : Notification.Subscription }

    type ChangeSensorState =
        { SensorStateUpdate : SensorStateUpdate }

    type ChangeSensorName = 
        { SensorId : SensorId
          DeviceGroupId : DeviceGroupId
          SensorName : string }
    
    type Command =
        | SubscribeToPushNotifications of SubscribeToPushNotifications
        | ChangeSensorState of ChangeSensorState
        | ChangeSensorName of ChangeSensorName

    let ChangeSensorState (sensorStateUpdate : SensorStateUpdate) : ChangeSensorState =
        {
            SensorStateUpdate = sensorStateUpdate
        }
    
    let From (sensorStateUpdates : SensorStateUpdate list) : Command list =
        sensorStateUpdates
        |> List.map (fun sensorStateUpdate -> ChangeSensorState sensorStateUpdate)
        |> List.map (fun changeSensorState -> Command.ChangeSensorState changeSensorState)

    let private subscribedToPushNotificationsEvent (command : SubscribeToPushNotifications) : Event.Event =
        let event : Event.SubscribedToPushNotifications =
            { DeviceGroupId = command.DeviceGroupId
              Subscription = command.Subscription }
        Event.SubscribedToPushNotifications event        
    
    let private sensorStateChangedEvent (command : ChangeSensorState) : Event.Event =   
        let sensorStateUpdate = command.SensorStateUpdate
        let event : Event.SensorStateChanged =
            { SensorId = sensorStateUpdate.SensorId
              DeviceGroupId = sensorStateUpdate.DeviceGroupId
              DeviceId = sensorStateUpdate.DeviceId
              Measurement = sensorStateUpdate.Measurement
              BatteryVoltage = sensorStateUpdate.BatteryVoltage
              SignalStrength = sensorStateUpdate.SignalStrength
              Timestamp = sensorStateUpdate.Timestamp }
        Event.SensorStateChanged event        

    let private sensorNameChangedEvent (command : ChangeSensorName) : Event.Event =
        let event : Event.SensorNameChanged =
            { SensorId = command.SensorId
              DeviceGroupId = command.DeviceGroupId
              SensorName = command.SensorName }
        Event.SensorNameChanged event

    let private createEventFromCommand (command : Command) : Event.Event =
        match command with
        | SubscribeToPushNotifications subscribeToPushNotifications -> subscribedToPushNotificationsEvent subscribeToPushNotifications 
        | ChangeSensorState changeSensorState -> sensorStateChangedEvent changeSensorState
        | ChangeSensorName changeSensorName -> sensorNameChangedEvent changeSensorName       
  
    let Execute (command : Command) =     
        async {
            let event = createEventFromCommand command
            do! Persistence.Store event
            do EventBus.Publish event
        }
