﻿namespace Jottai

[<AutoOpen>]
module SensorDataServiceClient = 
    open ApiObjects
    
    let PostDevicData token (deviceData : DeviceData) = 
        let apiUrl = "api/device-data"
        async {
            return! Http.Post token apiUrl deviceData
        }

    let PostMeasurement token deviceId (measurement : Measurement.Measurement) =        
        let deviceData = 
          { timestamp = ""
            gatewayId = ""
            channel = ""
            deviceId = deviceId
            deviceName = ""
            manufacturerName = ""
            data = []
            batteryVoltage = ""
            rssi = "" } 
            |> WithMeasurement(measurement)
        
        async { 
            return! PostDevicData token deviceData 
        }
