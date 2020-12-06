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
              scale = 1
              formattedValue = sprintf "%s C" value } : ApiObjects.SensorDatum
        | Measurement.RelativeHumidity relativeHumidity -> 
            let value = float(relativeHumidity).ToString(CultureInfo.InvariantCulture)
            { name = "RH"
              value = ""
              scale = 2
              formattedValue = sprintf "%s %%" value } : ApiObjects.SensorDatum
        | Measurement.PresenceOfWater presenceOfWater -> 
            { name = "DETECT"
              value = 
                  if presenceOfWater = Measurement.Present then "1"
                  else "0"
              scale = 0
              formattedValue = "" } : ApiObjects.SensorDatum
        | Measurement.Contact contact -> 
            let value = contact.ToString()
            { name = "CONTACT"
              value = 
                  if contact = Measurement.Open then "1"
                  else "0"
              scale = 0
              formattedValue = "" } : ApiObjects.SensorDatum
        | Measurement.Measurement.Motion motion -> 
            let value = motion.ToString()
            { name = "pir"
              value = 
                  if motion = Measurement.Motion then "1"
                  else "0"
              scale = 0
              formattedValue = "" } : ApiObjects.SensorDatum
        | Measurement.Voltage voltage ->
            let value = float(voltage).ToString(CultureInfo.InvariantCulture)
            { name = "voltage"
              value = value
              scale = 2
              formattedValue = "" } : ApiObjects.SensorDatum
        | Measurement.Rssi rssi -> 
            let value = float(rssi).ToString(CultureInfo.InvariantCulture)
            { name = "rssi"
              value = value
              scale = 0
              formattedValue = "" } : ApiObjects.SensorDatum
    
    let EmptySensorDataEvent() = 
        { event = "sensor data"
          gatewayId = ""
          channel = ""
          deviceId = ""
          data = []
          batteryVoltage = ""
          rssi = "" } : ApiObjects.SensorData
    
    let SensorDataEventWithSensorId sensorId = 
        { event = "sensor data"
          gatewayId = ""
          channel = ""
          deviceId = sensorId
          data = []
          batteryVoltage = ""
          rssi = "" } : ApiObjects.SensorData
    
    let WithMeasurement measurement sensorData =
        { sensorData with data = [ toDatum measurement ] } : ApiObjects.SensorData

