namespace Jottai

[<AutoOpen>]
module SensorDataServiceClient = 
    open ApiObjects
    
    let PostDeviceData token (deviceData : DeviceData) = 
        let apiUrl = "api/device-data"
        async {
            return! Http.Post token apiUrl deviceData
        }

    let PostMeasurement token deviceId (measurement : Measurement.Measurement) =        
        let deviceData = 
          { timestamp = ""
            gatewayId = ""            
            deviceId = deviceId
            deviceName = ""
            manufacturerName = ""
            data = []
            protocol = ApiObjects.Protocol.NotSpecified
            batteryVoltage = ""
            rssi = "" } 
            |> WithMeasurement(measurement)
        
        async { 
            return! PostDeviceData token deviceData 
        }
