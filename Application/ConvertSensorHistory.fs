namespace Jottai

module internal ConvertSensorHistory =

    let private ToEntry (entry : SensorHistoryStorage.StorableSensorHistoryEntry) : SensorHistoryEntry =
        let measuredValue = entry.MeasuredValue
        { MeasuredValue = measuredValue
          Timestamp = entry.Timestamp.ToUniversalTime() }
          
    let private ToHistoryEntries (stored : SensorHistoryStorage.StorableSensorHistory) : SensorHistoryEntry list =
         stored.Entries
         |> List.ofSeq
         |> List.map ToEntry

    let FromStorable(stored : SensorHistoryStorage.StorableSensorHistory) : SensorHistory =
        if stored :> obj |> isNull then
            EmptySensorHistory
        else
            { SensorId = stored.SensorId
              MeasuredProperty= stored.MeasuredProperty
              Entries = stored |> ToHistoryEntries }
        
    let ToApiObject (history : SensorHistory) : ApiObjects.SensorHistory =
        let entries =
            history.Entries
            |> List.map (fun entry ->
                let sensorHistoryResultEntry : ApiObjects.SensorHistoryEntry =
                  { MeasuredValue = entry.MeasuredValue
                    Timestamp = entry.Timestamp }
                sensorHistoryResultEntry
                )

        { SensorId = history.SensorId
          MeasuredProperty = history.MeasuredProperty
          Entries = entries }
