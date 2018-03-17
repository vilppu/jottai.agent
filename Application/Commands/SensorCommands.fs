﻿namespace YogRobot

module SensorCommands =
    
    let ChangeSensorState httpSend (command : ChangeSensorStateCommand) =
        let event =
            { SensorId = command.SensorId
              DeviceGroupId= command.DeviceGroupId
              DeviceId = command.DeviceId
              Measurement = command.Measurement
              BatteryVoltage = command.BatteryVoltage
              SignalStrength = command.SignalStrength
              Timestamp = command.Timestamp }

        async {
            do! SensorEventStorage.StoreSensorEvent event
            do! SensorStateChangedEventHandler.OnSensorStateChanged httpSend event
        }

    let ChangeSensorName (command : ChangeSensorNameCommand) =
    
        let event =
            { SensorId = command.SensorId
              DeviceGroupId = command.DeviceGroupId
              SensorName = command.SensorName }

        SensorSettingsEventHandler.OnSensorNameChanged event