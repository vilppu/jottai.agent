namespace Jottai

[<AutoOpen>]
module internal ConvertSensorHistory =
    open System.Collections.Generic
    open MongoDB.Bson

    let private ToEntry (entry : SensorHistoryStorage.StorableSensorHistoryEntry) : SensorHistoryEntry =
        let measuredValue = entry.MeasuredValue
        { MeasuredValue = measuredValue
          Timestamp = entry.Timestamp.ToUniversalTime() }
          
    let private ToHistoryEntries (stored : SensorHistoryStorage.StorableSensorHistory) : SensorHistoryEntry list =
         stored.Entries
         |> List.ofSeq
         |> List.map ToEntry
    
    let private EntryToStorable (entry : SensorHistoryEntry) : SensorHistoryStorage.StorableSensorHistoryEntry =
        { Id = ObjectId.Empty
          MeasuredValue = entry.MeasuredValue
          Timestamp = entry.Timestamp }

    let private UpdatedHistoryEntries (sensorState :  SensorState) (history : SensorHistory) =
        let maxNumberOfEntries = 30        
        let newEntry  = 
          { MeasuredValue = sensorState.Measurement |> Measurement.Value
            Timestamp = sensorState.LastUpdated }
        let newHistory = newEntry :: history.Entries
        newHistory
        |> List.truncate maxNumberOfEntries
        |> List.map EntryToStorable

    let FromStorable(stored : SensorHistoryStorage.StorableSensorHistory) : SensorHistory =
        if stored :> obj |> isNull then
            EmptySensorHistory
        else
            { PropertyId = stored.PropertyId
              MeasuredProperty= stored.MeasuredProperty
              Entries = stored |> ToHistoryEntries }
        
    let ToStorable (sensorState : SensorState) (history : SensorHistory)
        : SensorHistoryStorage.StorableSensorHistory =
        let updatedEntries = UpdatedHistoryEntries sensorState history
        
        { Id = ObjectId.Empty
          DeviceGroupId  = sensorState.DeviceGroupId.AsString
          PropertyId  = sensorState.PropertyId.AsString
          MeasuredProperty = sensorState.Measurement |> Measurement.Name
          Entries = new List<SensorHistoryStorage.StorableSensorHistoryEntry>(updatedEntries) }
 

