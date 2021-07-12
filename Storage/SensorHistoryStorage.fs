namespace Jottai

module SensorHistoryStorage =
    open System
    open MongoDB.Bson
    open MongoDB.Bson.Serialization.Attributes
    open MongoDB.Driver
    open Jottai.Expressions

    [<CLIMutable>]
    [<BsonIgnoreExtraElements>]
    type StorableSensorHistoryEntry = 
        { [<BsonIgnoreIfDefault>]
          Id : ObjectId
          MeasuredValue : obj
          Timestamp : DateTimeOffset }

    [<CLIMutable>]
    [<BsonIgnoreExtraElements>]
    type StorableSensorHistory = 
        { 
          [<BsonIgnoreIfDefault>]
          Id : ObjectId
          DeviceGroupId : string
          PropertyId : string
          MeasuredProperty : string
          Entries : Collections.Generic.List<StorableSensorHistoryEntry> }

    let private SensorHistoryCollectionName = "SensorHistory"
    
    let private ToEntry (entry : SensorHistoryEntry) : SensorHistoryEntry =
      let measuredValue = entry.MeasuredValue
      { MeasuredValue = measuredValue
        Timestamp = entry.Timestamp.ToUniversalTime() }
        
    let private EntryToStorable (entry : SensorHistoryEntry) : SensorHistoryEntry =
      { MeasuredValue = entry.MeasuredValue
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
            
    let private ToStorable (sensorState : SensorState) (history : SensorHistory)
        : SensorHistory =
        let updatedEntries = UpdatedHistoryEntries sensorState history
        { DeviceGroupId  = sensorState.DeviceGroupId
          PropertyId  = sensorState.PropertyId
          MeasuredProperty = sensorState.Measurement |> Measurement.Name
          Entries = updatedEntries }
     

    let private SensorHistoryCollection = 
        BsonStorage.Database.GetCollection<SensorHistory> SensorHistoryCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"        
        |> BsonStorage.WithDescendingIndex "DeviceId"
        |> BsonStorage.WithDescendingIndex "MeasuredProperty"
    
    let private FilterHistoryBy (deviceGroupId : DeviceGroupId) (propertyId : PropertyId) =
        let propertyId = propertyId
        let deviceGroupId = deviceGroupId
        let expr = Lambda.Create<SensorHistory>(fun x -> x.DeviceGroupId = deviceGroupId && x.PropertyId = propertyId)
        expr

    let GetSensorHistory (deviceGroupId : DeviceGroupId) (propertyId : PropertyId)
        : Async<SensorHistory> =
        async {
            let filter = FilterHistoryBy deviceGroupId propertyId
            let! history =
                SensorHistoryCollection.Find<SensorHistory>(filter).FirstOrDefaultAsync<SensorHistory>()
                |> Async.AwaitTask
            let historyOrEmpty =
                if history :> obj |> isNull
                then EmptySensorHistory
                else history
            return historyOrEmpty
        }
        
    let UpsertSensorHistory (sensorState : SensorState) (sensorHistory : SensorHistory) =       
       
      let storableSensorHistory = ToStorable sensorState sensorHistory
      let filter = FilterHistoryBy storableSensorHistory.DeviceGroupId storableSensorHistory.PropertyId
        
      SensorHistoryCollection.ReplaceOneAsync<SensorHistory>(filter, storableSensorHistory, BsonStorage.Replace)
      |> Async.AwaitTask
      |> Async.Ignore
    
    let Drop() = BsonStorage.Database.DropCollection(SensorHistoryCollectionName)