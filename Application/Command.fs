namespace Jottai

module internal Command =
   
    type SubscribeToPushNotifications =
        { DeviceGroupId : DeviceGroupId
          Subscription : Notification.Subscription }

    type ChangeSensorState =
        { SensorStateUpdate : SensorStateUpdate }

    type ChangeSensorName = 
        { PropertyId : PropertyId
          DeviceGroupId : DeviceGroupId
          PropertyName : PropertyName }

    type ChangeDevicePropertyState =
        { DeviceProperty : DevicePropertyStateUpdate }

    type RequestToChangeDeviceProperty =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId
          PropertyValue : DeviceProperty.DeviceProperty }

    type ChangeDevicePropertyName =
        { DeviceGroupId : DeviceGroupId
          GatewayId : GatewayId
          DeviceId : DeviceId
          PropertyId : PropertyId
          PropertyName : PropertyName }
    
    type Command =
        | SubscribeToPushNotifications of SubscribeToPushNotifications
        | ChangeSensorState of ChangeSensorState
        | ChangeSensorName of ChangeSensorName
        | ChangeDevicePropertyState of ChangeDevicePropertyState
        | RequestToChangeDevicePropertyValue of RequestToChangeDeviceProperty
        | ChangeDevicePropertyName of ChangeDevicePropertyName

    let private SubscribedToPushNotifications (command : SubscribeToPushNotifications) : Event.Event =
        let event : Event.SubscribedToPushNotifications =
            { DeviceGroupId = command.DeviceGroupId
              Subscription = command.Subscription }
        Event.SubscribedToPushNotifications event
    
    let private SensorStateChanged (command : ChangeSensorState) : Event.Event =   
        let sensorStateUpdate = command.SensorStateUpdate
        let event : Event.SensorStateChanged =
            { GatewayId = sensorStateUpdate.GatewayId
              PropertyId = sensorStateUpdate.PropertyId
              PropertyName = sensorStateUpdate.PropertyName
              PropertyDescription = sensorStateUpdate.PropertyDescription
              DeviceGroupId = sensorStateUpdate.DeviceGroupId
              DeviceId = sensorStateUpdate.DeviceId
              Measurement = sensorStateUpdate.Measurement
              Protocol = sensorStateUpdate.Protocol
              BatteryVoltage = sensorStateUpdate.BatteryVoltage
              SignalStrength = sensorStateUpdate.SignalStrength
              Timestamp = sensorStateUpdate.Timestamp }
        Event.SensorStateChanged event

    let private SensorNameChanged (command : ChangeSensorName) : Event.Event =
        let event : Event.SensorNameChanged =
            { PropertyId = command.PropertyId
              DeviceGroupId = command.DeviceGroupId
              PropertyName = command.PropertyName }
        Event.SensorNameChanged event
    
    let private DevicePropertyChanged (command : ChangeDevicePropertyState) : Event.Event =
        let event : Event.DevicePropertyChanged =
            { DeviceGroupId = command.DeviceProperty.DeviceGroupId
              GatewayId = command.DeviceProperty.GatewayId
              DeviceId = command.DeviceProperty.DeviceId
              PropertyId = command.DeviceProperty.PropertyId              
              PropertyName = command.DeviceProperty.PropertyName
              PropertyDescription = command.DeviceProperty.PropertyDescription
              PropertyValue = command.DeviceProperty.PropertyValue
              Protocol = command.DeviceProperty.Protocol
              Timestamp = command.DeviceProperty.Timestamp }
        Event.DevicePropertyChanged event
        
    let private ChangeDevicePropertyValueRequested (command : RequestToChangeDeviceProperty) : Event.Event =
        let event : Event.DevicePropertyChangeRequest=
           { DeviceGroupId = command.DeviceGroupId
             GatewayId = command.GatewayId
             DeviceId = command.DeviceId
             PropertyId = command.PropertyId
             PropertyValue = command.PropertyValue }
        Event.DevicePropertyChangeRequested event
        
    let private ChangeDevicePropertyNameRequested (command : ChangeDevicePropertyName) : Event.Event =
        let event : Event.DevicePropertyNameChanged=
           { DeviceGroupId = command.DeviceGroupId
             GatewayId = command.GatewayId
             DeviceId = command.DeviceId
             PropertyId = command.PropertyId
             PropertyName = command.PropertyName }
        Event.DevicePropertyNameChanged event

    let private CreateEventFromCommand (command : Command) : Event.Event =
        match command with
        | SubscribeToPushNotifications subscribeToPushNotifications -> SubscribedToPushNotifications subscribeToPushNotifications 
        | ChangeSensorState changeSensorState -> SensorStateChanged changeSensorState
        | ChangeSensorName changeSensorName -> SensorNameChanged changeSensorName
        | ChangeDevicePropertyState setDevicePropertyAvailable -> DevicePropertyChanged setDevicePropertyAvailable
        | RequestToChangeDevicePropertyValue changeDevicePropertyValue -> ChangeDevicePropertyValueRequested changeDevicePropertyValue
        | ChangeDevicePropertyName changeDevicePropertyName -> ChangeDevicePropertyNameRequested changeDevicePropertyName

    let FromDevicePropertyValue 
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
            |> RequestToChangeDevicePropertyValue
            |> Some
        | None -> None

    let FromDevicePropertyName
        (deviceGroupId : string)
        (gatewayId : string)
        (deviceId : string)
        (propertyId : string)
        (_ : string)
        (propertyName : string)
        : Command option =       
        
        let propertyName = ValidatePropertyName propertyName
        match propertyName with
        | Some propertyName ->
            { DeviceGroupId = DeviceGroupId deviceGroupId
              GatewayId = GatewayId gatewayId
              DeviceId = DeviceId deviceId
              PropertyId = PropertyId propertyId
              PropertyName = propertyName }
            |> ChangeDevicePropertyName
            |> Some
        | None -> None
  
    let Execute (command : Command) =     
        async {
            let event = CreateEventFromCommand command
            do! Persistence.Store event
            do EventBus.Publish event
        }
