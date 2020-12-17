namespace Jottai

module internal ConvertSensorStateUpdate =

    let ToStorable (update : SensorStateUpdate) : SensorEventStorage.StorableSensorEvent  =            
            { Id = MongoDB.Bson.ObjectId.Empty
              DeviceGroupId =  update.DeviceGroupId.AsString
              DeviceId = update.DeviceId.AsString
              SensorId = update.SensorId.AsString
              MeasuredProperty = update.Measurement |> Measurement.Name
              MeasuredValue = update.Measurement |> Measurement.Value
              Voltage = (float)update.BatteryVoltage
              SignalStrength = (float)update.SignalStrength
              Timestamp = update.Timestamp }
