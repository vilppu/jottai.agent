namespace Jottai

module internal ConvertSensorHistory =
        
    let ToApiObject (history : SensorHistory) : ApiObjects.SensorHistory =
        let entries =
            history.Entries
            |> List.map (fun entry ->
                let sensorHistoryResultEntry : ApiObjects.SensorHistoryEntry =
                  { MeasuredValue = entry.MeasuredValue
                    Timestamp = entry.Timestamp }
                sensorHistoryResultEntry
                )
        let (PropertyId propertyId) = history.PropertyId
        { PropertyId = propertyId
          MeasuredProperty = history.MeasuredProperty
          Entries = entries }
