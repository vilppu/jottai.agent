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

    type SetDevicePropertyAvailable =
        { DeviceProperty : DeviceProperty }

    type DevicePropertyChangeRequest =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId
          PropertyValue : DeviceProperty.DeviceProperty }
    
    type Command =
        | SubscribeToPushNotifications of SubscribeToPushNotifications
        | ChangeSensorState of ChangeSensorState
        | ChangeSensorName of ChangeSensorName
        | SetDevicePropertyAvailable of SetDevicePropertyAvailable
        | ChangeDevicePropertyValue of DevicePropertyChangeRequest

    let private SubscribedToPushNotificationsEvent (command : SubscribeToPushNotifications) : Event.Event =
        let event : Event.SubscribedToPushNotifications =
            { DeviceGroupId = command.DeviceGroupId
              Subscription = command.Subscription }
        Event.SubscribedToPushNotifications event
    
    let private SensorStateChangedEvent (command : ChangeSensorState) : Event.Event =   
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

    let private SensorNameChangedEvent (command : ChangeSensorName) : Event.Event =
        let event : Event.SensorNameChanged =
            { SensorId = command.SensorId
              DeviceGroupId = command.DeviceGroupId
              SensorName = command.SensorName }
        Event.SensorNameChanged event
    
    let private DevicePropertyChanged (command : SetDevicePropertyAvailable) : Event.Event =
        Event.DevicePropertyAvailable command.DeviceProperty
        
    let private ChangeDevicePropertyValueRequested (command : DevicePropertyChangeRequest) : Event.Event =
        let event : Devices.DevicePropertyChangeRequest=
           { DeviceGroupId = command.DeviceGroupId
             GatewayId = command.GatewayId
             DeviceId = command.DeviceId
             PropertyId = command.PropertyId
             PropertyValue = command.PropertyValue }
        Event.DevicePropertyChangeRequested event

    let private CreateEventFromCommand (command : Command) : Event.Event =
        match command with
        | SubscribeToPushNotifications subscribeToPushNotifications -> SubscribedToPushNotificationsEvent subscribeToPushNotifications 
        | ChangeSensorState changeSensorState -> SensorStateChangedEvent changeSensorState
        | ChangeSensorName changeSensorName -> SensorNameChangedEvent changeSensorName
        | SetDevicePropertyAvailable setDevicePropertyAvailable -> DevicePropertyChanged setDevicePropertyAvailable
        | ChangeDevicePropertyValue devicePropertyChangeRequest -> ChangeDevicePropertyValueRequested devicePropertyChangeRequest

    let FromDeviceProperty 
        (deviceGroupId : string)
        (gatewayId : string)
        (deviceId : string)
        (propertyId : string)
        (propertyType : string)
        (propertyValue : string)
        : Command option =
        
        let propertyValue = DeviceProperty.FromString propertyType propertyValue

        match propertyValue with
        | Some propertyValue ->
            { DeviceGroupId = DeviceGroupId deviceGroupId
              GatewayId = GatewayId gatewayId
              DeviceId = DeviceId deviceId
              PropertyId = PropertyId propertyId
              PropertyValue = propertyValue }
            |> ChangeDevicePropertyValue
            |> Some
        | None -> None
  
    let Execute (command : Command) =     
        async {
            let event = CreateEventFromCommand command
            do! Persistence.Store event
            do EventBus.Publish event
        }
