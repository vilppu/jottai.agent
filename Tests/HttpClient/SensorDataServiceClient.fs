namespace Jottai

[<AutoOpen>]
module SensorDataServiceClient = 
    open ApiObjects
    
    let PostSensorData token (sensorData : SensorData) = 
        let apiUrl = "api/sensor-data"
        async {
            return! Http.Post token apiUrl sensorData            
        }

    let PostMeasurement token deviceId (measurement : Measurement.Measurement) =        
        let sensorData = 
          { event = "sensor data"
            gatewayId = ""
            channel = ""
            deviceId = deviceId
            data = []
            batteryVoltage = ""
            rssi = "" }
        let event = sensorData |> WithMeasurement(measurement)
        async { 
            return! PostSensorData token event 
        }
