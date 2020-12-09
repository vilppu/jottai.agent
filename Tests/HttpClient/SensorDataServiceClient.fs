namespace Jottai

[<AutoOpen>]
module SensorDataServiceClient = 
    open ApiObjects
    
    let PostSensorData token (deviceData : DeviceData) = 
        let apiUrl = "api/sensor-data"
        async {
            return! Http.Post token apiUrl deviceData            
        }

    let PostMeasurement token deviceId (measurement : Measurement.Measurement) =        
        let deviceData = 
          { timestamp = ""
            gatewayId = ""
            channel = ""
            deviceId = deviceId
            data = []
            availableCommands = []
            batteryVoltage = ""
            rssi = "" }
        let event = deviceData |> WithMeasurement(measurement)
        async { 
            return! PostSensorData token event 
        }
