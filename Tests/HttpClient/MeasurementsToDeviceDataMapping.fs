namespace Jottai

[<AutoOpen>]
module MeasurementsToDeviceDataMapping = 
    open System.Globalization
    
    let private ToDatum measurement = 
        match measurement with
        | Measurement.Temperature temperature -> 
            let value = float(temperature).ToString(CultureInfo.InvariantCulture)
            { propertyId = null
              propertyName = "TEMPERATURE"
              propertyDescription = ""
              protocol = null
              propertyTypeId = null
              value = ""
              valueType = null
              unitOfMeasurement = ""
              formattedValue = sprintf "%s C" value
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.RelativeHumidity relativeHumidity -> 
            let value = float(relativeHumidity).ToString(CultureInfo.InvariantCulture)
            { propertyId = null
              propertyName = "RH"
              propertyDescription = ""
              protocol = null
              propertyTypeId = null
              value = ""
              valueType = null
              unitOfMeasurement = ""
              formattedValue = sprintf "%s %%" value
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.PresenceOfWater presenceOfWater -> 
            { propertyId = null
              propertyName = "DETECT"
              propertyDescription = ""
              protocol = null
              propertyTypeId = null
              value = 
                  if presenceOfWater = Measurement.Present then "1"
                  else "0"
              valueType = null
              unitOfMeasurement = ""
              formattedValue = ""
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Contact contact -> 
            let value = contact.ToString()
            { propertyId = null
              propertyName = "CONTACT"
              propertyDescription = ""
              protocol = null
              propertyTypeId = null
              value = 
                  if contact = Measurement.Open then "1"
                  else "0"              
              valueType = null
              unitOfMeasurement = ""
              formattedValue = ""
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Measurement.Motion motion ->             
            { propertyId = null
              propertyName = "pir"
              propertyDescription = ""
              protocol = null
              propertyTypeId = null
              value = 
                  if motion = Measurement.Motion then "1"
                  else "0"              
              valueType = null
              unitOfMeasurement = ""
              formattedValue = ""
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Voltage voltage ->
            let value = float(voltage).ToString(CultureInfo.InvariantCulture)
            { propertyId = null
              propertyName = "voltage"
              propertyDescription = ""
              protocol = null
              propertyTypeId = null
              value = value              
              valueType = null
              unitOfMeasurement = ""
              formattedValue = ""
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Rssi rssi -> 
            let value = float(rssi).ToString(CultureInfo.InvariantCulture)
            { propertyId = null
              propertyName = "rssi"
              propertyDescription = ""
              protocol = null
              propertyTypeId = null
              value = value              
              valueType = null
              unitOfMeasurement = ""
              formattedValue = ""
              minimumValue = ""
              maximumValue = "" } : ApiObjects.DeviceDatum
            : ApiObjects.DeviceDatum
    
    let SensorDataEventWithDeviceId deviceId = 
        { timestamp = ""
          gatewayId = ""
          channel = ""
          deviceId = deviceId
          deviceName = ""
          manufacturerName = null
          data = []
          batteryVoltage = ""
          rssi = "" } : ApiObjects.DeviceData
    
    let WithMeasurement measurement deviceData =
        { deviceData with data = [ ToDatum measurement ] } : ApiObjects.DeviceData
