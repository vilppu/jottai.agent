namespace Jottai

[<AutoOpen>]
module internal ConvertSensorHistory =
    open System.Collections.Generic
    open MongoDB.Bson

    let private toEntry (entry : SensorHistoryStorage.StorableSensorHistoryEntry) : SensorHistoryEntry =
        let measuredValue = entry.MeasuredValue
        { MeasuredValue = measuredValue
          Timestamp = entry.Timestamp.ToUniversalTime() }
          
    let private toHistoryEntries (stored : SensorHistoryStorage.StorableSensorHistory) : SensorHistoryEntry list =
         stored.Entries
         |> List.ofSeq
         |> List.map toEntry
    
    let private entryToStorable (entry : SensorHistoryEntry) : SensorHistoryStorage.StorableSensorHistoryEntry =
        { Id = ObjectId.Empty
          MeasuredValue = entry.MeasuredValue
          Timestamp = entry.Timestamp }

    let private updatedHistoryEntries (sensorState :  SensorState) (history : SensorHistory) =
        let maxNumberOfEntries = 30        
        let newEntry  = 
          { MeasuredValue = sensorState.Measurement |> Value
            Timestamp = sensorState.LastUpdated }
        let newHistory = newEntry :: history.Entries
        newHistory
        |> List.truncate maxNumberOfEntries
        |> List.map entryToStorable

    let FromStorable(stored : SensorHistoryStorage.StorableSensorHistory) : SensorHistory =
        if stored :> obj |> isNull then
            EmptySensorHistory
        else
            { SensorId = stored.SensorId
              MeasuredProperty= stored.MeasuredProperty
              Entries = stored |> toHistoryEntries }
        
    let ToStorable (sensorState : SensorState) (history : SensorHistory)
        : SensorHistoryStorage.StorableSensorHistory =
        let updatedEntries = updatedHistoryEntries sensorState history
        
        { Id = ObjectId.Empty
          DeviceGroupId  = sensorState.DeviceGroupId.AsString
          SensorId  = sensorState.SensorId.AsString
          MeasuredProperty = sensorState.Measurement |> Name
          Entries = new List<SensorHistoryStorage.StorableSensorHistoryEntry>(updatedEntries) }
        
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
        
    let ToApiObjects (statuses : SensorState list) : ApiObjects.SensorState list = 
        statuses
        |> List.map (fun sensorState ->            
            { DeviceGroupId = sensorState.DeviceGroupId.AsString
              DeviceId = sensorState.DeviceId.AsString
              SensorId = sensorState.SensorId.AsString
              SensorName = sensorState.SensorName
              MeasuredProperty = sensorState.Measurement |> Name
              MeasuredValue = sensorState.Measurement |> Value
              BatteryVoltage = float(sensorState.BatteryVoltage)
              SignalStrength = sensorState.SignalStrength
              LastUpdated = sensorState.LastUpdated
              LastActive = sensorState.LastActive })
