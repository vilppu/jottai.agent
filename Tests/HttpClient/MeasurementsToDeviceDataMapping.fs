namespace Jottai

[<AutoOpen>]
module MeasurementsToDeviceDataMapping = 
    open System.Globalization
    
    let private ToDatum measurement = 
        match measurement with
        | Measurement.Temperature temperature -> 
            let value = float(temperature).ToString(CultureInfo.InvariantCulture)
            { propertyId = null
              propertyType = ApiObjects.PropertyType.Temperature
              propertyName = "TEMPERATURE"
              propertyDescription = ""
              value = value
              valueType = ApiObjects.ValueType.Decimal
              unitOfMeasurement = ""
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.RelativeHumidity relativeHumidity -> 
            let value = float(relativeHumidity).ToString(CultureInfo.InvariantCulture)
            { propertyId = null
              propertyType = ApiObjects.PropertyType.RelativeHumidity
              propertyName = "RH"
              propertyDescription = ""
              value = value
              valueType = ApiObjects.ValueType.Decimal
              unitOfMeasurement = ""
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.PresenceOfWater presenceOfWater -> 
            { propertyId = null
              propertyName = "DETECT"
              propertyDescription = ""
              propertyType = ApiObjects.PropertyType.PresenceOfWater
              value = 
                  if presenceOfWater = Measurement.Present then "1"
                  else "0"
              valueType = ApiObjects.ValueType.Integer
              unitOfMeasurement = ""
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Contact contact -> 
            let value = contact.ToString()
            { propertyId = null
              propertyName = "CONTACT"
              propertyDescription = ""
              propertyType = ApiObjects.PropertyType.Contact
              value = 
                  if contact = Measurement.Open then "1"
                  else "0"              
              valueType = ApiObjects.ValueType.Integer
              unitOfMeasurement = ""              
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Measurement.Motion motion ->             
            { propertyId = null
              propertyName = "pir"
              propertyDescription = ""
              propertyType = ApiObjects.PropertyType.Motion
              value = 
                  if motion = Measurement.Motion then "1"
                  else "0"              
              valueType = ApiObjects.ValueType.Integer
              unitOfMeasurement = ""              
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Voltage voltage ->
            let value = float(voltage).ToString(CultureInfo.InvariantCulture)
            { propertyId = null
              propertyName = "voltage"
              propertyDescription = ""
              propertyType = ApiObjects.PropertyType.Voltage
              value = value              
              valueType = ApiObjects.ValueType.Decimal
              unitOfMeasurement = ""              
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Rssi rssi -> 
            let value = float(rssi).ToString(CultureInfo.InvariantCulture)
            { propertyId = null
              propertyName = "rssi"
              propertyDescription = ""
              propertyType = ApiObjects.PropertyType.Rssi
              value = value              
              valueType = ApiObjects.ValueType.Decimal
              unitOfMeasurement = ""              
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | _ ->
            { propertyId = null
              propertyName = ""
              propertyDescription = ""
              propertyType = ApiObjects.PropertyType.Sensor
              value = null              
              valueType = null
              unitOfMeasurement = ""
              formattedValue = ""
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
    
    let SensorDataEventWithDeviceId deviceId = 
        { timestamp = ""
          gatewayId = ""
          deviceId = deviceId
          deviceName = ""
          manufacturerName = null          
          data = []
          protocol = ApiObjects.Protocol.NotSpecified
          batteryVoltage = ""
          rssi = "" } : ApiObjects.DeviceData
    
    let WithMeasurement measurement deviceData =
        { deviceData with data = [ ToDatum measurement ] } : ApiObjects.DeviceData
