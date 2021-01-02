namespace Jottai

module ZWavePlus =

    type private CommandClass =
        | Alarm
        | ApplicationStatus
        | Association
        | AssociationCommandConfiguration
        | BarrierOperator
        | Basic
        | BasicWindowCovering
        | Battery
        | CentralScene
        | ClimateControlSchedule
        | Clock
        | Color
        | Configuration
        | ControllerReplication 
        | DeviceResetLocally
        | DoorLock
        | DoorLockLogging
        | EnergyProduction
        | Hail
        | Indicator
        | Language
        | Lock
        | ManufacturerProprietary
        | ManufacturerSpecific
        | Meter
        | MeterPulse
        | MultiChannelAssociation
        | MultiCmd
        | MultiInstance
        | NodeNaming
        | NoOperation
        | Powerlevel
        | Proprietary
        | Protection
        | SceneActivation
        | Security
        | SensorAlarm
        | SensorBinary
        | SensorMultilevel
        | SoundSwitch
        | SwitchAll
        | SwitchBinary
        | SwitchMultilevel
        | SwitchToggleBinary
        | SwitchToggleMultilevel
        | ThermostatFanMode
        | ThermostatFanState
        | ThermostatMode
        | ThermostatOperatingState
        | ThermostatSetpoint
        | TimeParameters
        | UserCode
        | Version
        | WakeUp
        | ZWavePlusInfo
        | UnknownCommandClass

    let private ToCommandClass commandClassId =
        match commandClassId with
        | 0x71 -> Alarm
        | 0x22 -> ApplicationStatus
        | 0x85 -> Association
        | 0x9b -> AssociationCommandConfiguration
        | 0x66 -> BarrierOperator
        | 0x20 -> Basic
        | 0x50 -> BasicWindowCovering
        | 0x80 -> Battery
        | 0x5B -> CentralScene
        | 0x46 -> ClimateControlSchedule
        | 0x81 -> Clock
        | 0x33 -> Color
        | 0x70 -> Configuration
        | 0x21 -> ControllerReplication 
        | 0x5a -> DeviceResetLocally
        | 0x62 -> DoorLock
        | 0x4C -> DoorLockLogging
        | 0x90 -> EnergyProduction
        | 0x82 -> Hail
        | 0x87 -> Indicator
        | 0x89 -> Language
        | 0x76 -> Lock
        | 0x91 -> ManufacturerProprietary
        | 0x72 -> ManufacturerSpecific
        | 0x32 -> Meter
        | 0x35 -> MeterPulse
        | 0x8E -> MultiChannelAssociation
        | 0x8f -> MultiCmd
        | 0x60 -> MultiInstance
        | 0x77 -> NodeNaming
        | 0x00 -> NoOperation
        | 0x73 -> Powerlevel
        | 0x88 -> Proprietary
        | 0x75 -> Protection
        | 0x2B -> SceneActivation
        | 0x98 -> Security
        | 0x9c -> SensorAlarm
        | 0x30 -> SensorBinary
        | 0x31 -> SensorMultilevel
        | 0x79 -> SoundSwitch
        | 0x27 -> SwitchAll
        | 0x25 -> SwitchBinary
        | 0x26 -> SwitchMultilevel
        | 0x28 -> SwitchToggleBinary
        | 0x29 -> SwitchToggleMultilevel
        | 0x44 -> ThermostatFanMode
        | 0x45 -> ThermostatFanState
        | 0x40 -> ThermostatMode
        | 0x42 -> ThermostatOperatingState
        | 0x43 -> ThermostatSetpoint
        | 0x8B -> TimeParameters
        | 0x63 -> UserCode
        | 0x86 -> Version
        | 0x84 -> WakeUp
        | 0x5E -> ZWavePlusInfo
        | _ -> UnknownCommandClass

    let private ParseTimestamp timestamp =
        if System.String.IsNullOrWhiteSpace(timestamp)
        then System.DateTimeOffset.UtcNow
        else System.DateTimeOffset.Parse(timestamp)

    let private ParseCommandClass (datum : ApiObjects.DeviceDatum) : CommandClass =
        let commandClassIdIsInteger, commandClassId =  System.Int32.TryParse datum.propertyTypeId
        if commandClassIdIsInteger
        then commandClassId |> ToCommandClass
        else UnknownCommandClass

    let private ParseBinarySwitch (datum : ApiObjects.DeviceDatum) : DeviceProperty.DeviceProperty option =
        let valueIsBoolean, isOn = System.Boolean.TryParse(datum.value)
        match valueIsBoolean, isOn with
        | true, true -> DeviceProperty.BinarySwitch DeviceProperty.On |> Some
        | true, false -> DeviceProperty.BinarySwitch DeviceProperty.Off |> Some
        | _ -> None

    let private ToDevicePropertyUpdate
        (deviceGroupId : DeviceGroupId)
        (deviceData : ApiObjects.DeviceData)
        (datum : ApiObjects.DeviceDatum)
        (timestamp)
        (propertyType)
        (propertyValue)
        : DeviceDataUpdate option =
        { DeviceGroupId = deviceGroupId
          GatewayId = GatewayId deviceData.gatewayId
          DeviceId = DeviceId deviceData.deviceId
          PropertyId = PropertyId datum.propertyId
          PropertyType = propertyType
          PropertyName = PropertyName datum.propertyName
          PropertyDescription = PropertyDescription datum.propertyDescription
          PropertyValue = propertyValue
          Protocol = ZWavePlus
          Timestamp = timestamp }
        |> DevicePropertyUpdate
        |> Some

    let private ParseDeviceProperty
        (datum : ApiObjects.DeviceDatum)
        : DeviceProperty.DeviceProperty option =        
        let commandClass = datum |> ParseCommandClass
        match commandClass with
        | SwitchBinary -> datum |> ParseBinarySwitch
        | _ -> None

    let private ToPropertyType (commandClass : CommandClass) : PropertyType option =
        match commandClass with
        | SwitchBinary -> BinarySwitch |> Some
        | _ -> None

    let ToDeviceDataUpdate
        (deviceGroupId : DeviceGroupId)
        (deviceData : ApiObjects.DeviceData)
        (datum : ApiObjects.DeviceDatum)
        : DeviceDataUpdate option =
        let commandClass = datum |> ParseCommandClass
        let propertyType = commandClass |> ToPropertyType
        let timestamp = deviceData.timestamp |> ParseTimestamp
        let deviceProperty = datum |> ParseDeviceProperty
        let toDevicePropertyUpdate = ToDevicePropertyUpdate deviceGroupId deviceData datum timestamp
        match (propertyType, deviceProperty) with
        | (Some propertyType, Some deviceProperty) -> toDevicePropertyUpdate propertyType deviceProperty
        | _ -> None
