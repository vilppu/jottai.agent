namespace Jottai

[<AutoOpen>]
module MeasurementsToSensorDataMapping = 
    open System.Globalization
    
    let private toDatum measurement = 
        match measurement with
        | Measurement.Temperature temperature -> 
            let value = float(temperature).ToString(CultureInfo.InvariantCulture)
            { name = "TEMPERATURE"
              value = ""
              unit = ""
              formattedValue = sprintf "%s C" value } : ApiObjects.DeviceDatum
        | Measurement.RelativeHumidity relativeHumidity -> 
            let value = float(relativeHumidity).ToString(CultureInfo.InvariantCulture)
            { name = "RH"
              value = ""
              unit = ""
              formattedValue = sprintf "%s %%" value } : ApiObjects.DeviceDatum
        | Measurement.PresenceOfWater presenceOfWater -> 
            { name = "DETECT"
              value = 
                  if presenceOfWater = Measurement.Present then "1"
                  else "0"
              unit = ""
              formattedValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Contact contact -> 
            let value = contact.ToString()
            { name = "CONTACT"
              value = 
                  if contact = Measurement.Open then "1"
                  else "0"
              unit = ""
              formattedValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Measurement.Motion motion -> 
            let value = motion.ToString()
            { name = "pir"
              value = 
                  if motion = Measurement.Motion then "1"
                  else "0"
              unit = ""
              formattedValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Voltage voltage ->
            let value = float(voltage).ToString(CultureInfo.InvariantCulture)
            { name = "voltage"
              value = value
              unit = ""
              formattedValue = "" } : ApiObjects.DeviceDatum
        | Measurement.Rssi rssi -> 
            let value = float(rssi).ToString(CultureInfo.InvariantCulture)
            { name = "rssi"
              value = value
              unit = ""
              formattedValue = "" } : ApiObjects.DeviceDatum

        | Measurement.Unsupported  ->
            { name = ""
              value = ""
              unit = ""
              formattedValue = "" } : ApiObjects.DeviceDatum
    
    let SensorDataEventWithSensorId sensorId = 
        { timestamp = ""
          gatewayId = ""
          channel = ""
          deviceId = sensorId
          data = []
          availableCommands = []
          batteryVoltage = ""
          rssi = "" } : ApiObjects.DeviceData
    
    let WithMeasurement measurement deviceData =
        { deviceData with data = [ toDatum measurement ] } : ApiObjects.DeviceData

