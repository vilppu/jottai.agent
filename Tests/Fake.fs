namespace Jottai

module Fake = 
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    
    let private exampleMeasurement = Measurement.Temperature 15.0<C>
    let Measurement measurement = (measurement, "ExampleDevice")
    let MeasurementFromDevice measurement deviceId = (measurement, deviceId)
    let SomeMeasurementFromDevice deviceId = (exampleMeasurement, deviceId)
    let DeviceId = "ExampleSensor"   
    
    let SomeSensorData : ApiObjects.SensorData = 
        { event = "sensor data"
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = []
          batteryVoltage = ""
          rssi = "" }
    
    let SensorEventWithEmptyDatumValues : ApiObjects.SensorData = 
        let data : ApiObjects.SensorDatum list = 
          [ { name = "CONTACT"
              value = ""
              scale = 0
              formattedValue = "" }
            { name = "TEMPERATURE"
              value = null
              scale = 0
              formattedValue = "" }
            { name = "TEMPERATURE"
              value = ""
              scale = 0
              formattedValue = "" } ]

        { event = "sensor data"
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = data
          batteryVoltage = ""
          rssi = "" }
    
    let SensorEventWithInvalidDatumValues : ApiObjects.SensorData = 
        let data : ApiObjects.SensorDatum list = 
              [ { name = "TEMPERATURE"
                  value = "INVALID"
                  scale = 0
                  formattedValue = "" } ]
        { event = "sensor data"
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = data
          batteryVoltage = ""
          rssi = "" }
    
    let SensorEventWithInvalidDeviceProperties : ApiObjects.SensorData = 
        { event = "sensor data"
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = []
          batteryVoltage = "INVALID"
          rssi = "INVALID" }
    
    let SensorEventWithUnknownDatumValues : ApiObjects.SensorData = 
        let data : ApiObjects.SensorDatum list =
              [ { name = ""
                  value = "1"
                  scale = 0
                  formattedValue = "" }
                { name = null
                  value = "2"
                  scale = 0
                  formattedValue = "" }
                { name = "SOMETHING"
                  value = "2"
                  scale = 0
                  formattedValue = "" } ]

        { event = "sensor data"
          gatewayId = ""
          channel = ""
          deviceId = DeviceId
          data = data
          batteryVoltage = ""
          rssi = "" }
