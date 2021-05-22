namespace Jottai

module Fake = 
    open Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols
    
    let private ExampleMeasurement = Measurement.Temperature 15.0<C>
    let Measurement measurement = (measurement, "ExampleDevice")
    let MeasurementFromDevice measurement deviceId = (measurement, deviceId)
    let SomeMeasurementFromDevice deviceId = (ExampleMeasurement, deviceId)
    let MeasurementWithDeviceProperty deviceProperty = (deviceProperty, "ExampleDevice")
    let DeviceId = "ExampleSensor"
    
    let DeviceData : ApiObjects.DeviceData = 
        { timestamp = ""
          gatewayId = ""          
          deviceId = DeviceId
          deviceName = ""
          manufacturerName = null
          data = []          
          protocol = ApiObjects.Protocol.NotSpecified
          batteryVoltage = ""
          rssi = "" }
    
    let SensorEventWithEmptyDatumValues : ApiObjects.DeviceData = 
        let data : ApiObjects.DeviceDatum list = 
          [ { propertyId = null
              propertyName = "CONTACT"
              propertyDescription = ""
              propertyType = ApiObjects.PropertyType.Contact
              unitOfMeasurement = null
              value = ""
              valueType = ApiObjects.ValueType.Integer
              minimumValue = ""
              maximumValue = "" }
            { propertyId = null
              propertyName = "TEMPERATURE"
              propertyDescription = ""              
              propertyType = ApiObjects.PropertyType.Temperature
              unitOfMeasurement = null
              value = null
              valueType = ApiObjects.ValueType.Decimal
              minimumValue = ""
              maximumValue = "" }
            { propertyId = null
              propertyName = "TEMPERATURE"
              propertyDescription = ""
              unitOfMeasurement = null
              value = ""
              valueType = ApiObjects.ValueType.Decimal
              minimumValue = ""
              maximumValue = ""
              propertyType = ApiObjects.PropertyType.Temperature } ]

        { timestamp = ""
          gatewayId = ""
          deviceId = DeviceId
          deviceName = ""
          manufacturerName = ""
          data = data
          protocol = ApiObjects.Protocol.NotSpecified
          batteryVoltage = ""
          rssi = "" }
    
    let SensorEventWithInvalidDatumValues : ApiObjects.DeviceData = 
        let data : ApiObjects.DeviceDatum list = 
              [ { propertyId = null
                  propertyName = "TEMPERATURE"
                  propertyDescription = ""
                  propertyType = ApiObjects.PropertyType.Temperature
                  unitOfMeasurement = null
                  value = "INVALID"
                  valueType = ApiObjects.ValueType.Decimal
                  minimumValue = ""
                  maximumValue = "" } ]
        { timestamp = ""
          gatewayId = ""
          deviceId = DeviceId
          deviceName = ""
          manufacturerName = ""
          data = data
          protocol = ApiObjects.Protocol.NotSpecified
          batteryVoltage = ""
          rssi = "" }
    
    let SensorEventWithInvalidDeviceProperties : ApiObjects.DeviceData = 
        { timestamp = ""
          gatewayId = ""          
          deviceId = DeviceId
          deviceName = ""
          manufacturerName = ""
          data = []
          protocol = ApiObjects.Protocol.NotSpecified
          batteryVoltage = "INVALID"
          rssi = "INVALID" }

    let ZWavePlusDevicePropertyDatum : ApiObjects.DeviceDatum =
            { propertyId = "0x0002000001dc8013"
              propertyName = "Example device part"
              propertyDescription = "Example device part description"              
              propertyType = ApiObjects.PropertyType.TwoWaySwitch
              unitOfMeasurement = null
              value = "True"
              valueType = ApiObjects.ValueType.Boolean
              minimumValue = "0"
              maximumValue = "0"
              }