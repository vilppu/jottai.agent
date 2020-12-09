namespace Jottai

module Fake = 
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    
    let private exampleMeasurement = Measurement.Temperature 15.0<C>
    let Measurement measurement = (measurement, "ExampleDevice")
    let MeasurementFromDevice measurement deviceId = (measurement, deviceId)
    let SomeMeasurementFromDevice deviceId = (exampleMeasurement, deviceId)
    let DeviceId = "ExampleSensor"   
    
    let SomeSensorData : ApiObjects.DeviceData = 
        { timestamp = ""
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = []
          availableCommands = []
          batteryVoltage = ""
          rssi = "" }
    
    let SensorEventWithEmptyDatumValues : ApiObjects.DeviceData = 
        let data : ApiObjects.DeviceDatum list = 
          [ { name = "CONTACT"
              unit = ""
              value = ""
              formattedValue = "" }
            { name = "TEMPERATURE"
              unit = ""
              value = null
              formattedValue = "" }
            { name = "TEMPERATURE"
              unit = ""
              value = ""
              formattedValue = "" } ]

        { timestamp = ""
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = data
          availableCommands = []
          batteryVoltage = ""
          rssi = "" }
    
    let SensorEventWithInvalidDatumValues : ApiObjects.DeviceData = 
        let data : ApiObjects.DeviceDatum list = 
              [ { name = "TEMPERATURE"
                  unit = ""
                  value = "INVALID"
                  formattedValue = "" } ]
        { timestamp = ""
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = data
          availableCommands = []
          batteryVoltage = ""
          rssi = "" }
    
    let SensorEventWithInvalidDeviceProperties : ApiObjects.DeviceData = 
        { timestamp = ""
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = []
          availableCommands = []
          batteryVoltage = "INVALID"
          rssi = "INVALID" }
    
    let SensorEventWithUnknownDatumValues : ApiObjects.DeviceData = 
        let data : ApiObjects.DeviceDatum list =
              [ { name = ""
                  unit = ""
                  value = "1"
                  formattedValue = "" }
                { name = null
                  unit = ""
                  value = "2"
                  formattedValue = "" }
                { name = "SOMETHING"
                  unit = ""
                  value = "2"
                  formattedValue = "" } ]

        { timestamp = ""
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = data
          availableCommands = []
          batteryVoltage = ""
          rssi = "" }
