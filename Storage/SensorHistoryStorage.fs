namespace Jottai

module SensorHistoryStorage =
    open System
    open System.Linq
    open MongoDB.Bson
    open MongoDB.Bson.Serialization.Attributes
    open MongoDB.Driver
    open Jottai.Expressions

    [<CLIMutable>]
    [<BsonIgnoreExtraElements>]
    type StorableSensorHistoryEntry =
        { [<BsonIgnoreIfDefault>]
          Id: ObjectId
          MeasuredValue: obj
          Timestamp: DateTimeOffset }

    [<CLIMutable>]
    [<BsonIgnoreExtraElements>]
    type StorableSensorHistory =
        { [<BsonIgnoreIfDefault>]
          Id: ObjectId
          DeviceGroupId: string
          PropertyId: string
          MeasuredProperty: string
          Entries: Collections.Generic.List<StorableSensorHistoryEntry> }

    let private SensorHistoryCollectionName = "SensorHistory"

    let private SensorHistoryEntry (storableSensorHistoryEntry: StorableSensorHistoryEntry) : SensorHistoryEntry =

        { MeasuredValue = storableSensorHistoryEntry.MeasuredValue
          Timestamp = storableSensorHistoryEntry.Timestamp }

    let private SensorHistory (storableSensorHistory: StorableSensorHistory) : SensorHistory =

        { DeviceGroupId = DeviceGroupId storableSensorHistory.DeviceGroupId
          PropertyId = PropertyId storableSensorHistory.PropertyId
          MeasuredProperty = storableSensorHistory.MeasuredProperty
          Entries =
              storableSensorHistory.Entries
              |> Seq.map SensorHistoryEntry
              |> Seq.toList }

    let private EntryToStorable (entry: SensorHistoryEntry) : StorableSensorHistoryEntry =
        { Id = new ObjectId()
          MeasuredValue = entry.MeasuredValue
          Timestamp = entry.Timestamp }

    let private UpdatedHistoryEntries (sensorState: SensorState) (history: SensorHistory) =
        let maxNumberOfEntries = 30

        let newEntry =
            { MeasuredValue = sensorState.Measurement |> Measurement.Value
              Timestamp = sensorState.LastUpdated }

        let newHistory = newEntry :: history.Entries

        let entries =
            newHistory
            |> List.truncate maxNumberOfEntries
            |> List.map EntryToStorable

        entries.ToList()

    let private ToStorable (sensorState: SensorState) (history: SensorHistory) : StorableSensorHistory =
        let updatedEntries =
            UpdatedHistoryEntries sensorState history

        let (DeviceGroupId deviceGroupId) = sensorState.DeviceGroupId
        let (PropertyId propertyId) = sensorState.PropertyId

        { Id = new ObjectId()
          DeviceGroupId = deviceGroupId
          PropertyId = propertyId
          MeasuredProperty = sensorState.Measurement |> Measurement.Name
          Entries = updatedEntries }


    let private SensorHistoryCollection =
        BsonStorage.Database.GetCollection<StorableSensorHistory> SensorHistoryCollectionName
        |> BsonStorage.WithDescendingIndex "DeviceGroupId"
        |> BsonStorage.WithDescendingIndex "DeviceId"
        |> BsonStorage.WithDescendingIndex "MeasuredProperty"

    let private FilterHistoryBy (deviceGroupId: DeviceGroupId) (propertyId: PropertyId) =
        let (DeviceGroupId deviceGroupId) = deviceGroupId
        let (PropertyId propertyId) = propertyId

        let expr =
            Lambda.Create<StorableSensorHistory>
                (fun x ->
                    x.DeviceGroupId = deviceGroupId
                    && x.PropertyId = propertyId)

        expr

    let GetSensorHistory (deviceGroupId: DeviceGroupId) (propertyId: PropertyId) : Async<SensorHistory> =
        async {
            let filter = FilterHistoryBy deviceGroupId propertyId

            let! history =
                SensorHistoryCollection
                    .Find<StorableSensorHistory>(filter)
                    .ToListAsync<StorableSensorHistory>()
                |> Async.AwaitTask

            return
                if history.Count = 0 then
                    EmptySensorHistory
                else
                    history.[0] |> SensorHistory
        }

    let UpsertSensorHistory (sensorState: SensorState) (sensorHistory: SensorHistory) =

        let storableSensorHistory = ToStorable sensorState sensorHistory

        let filter =
            FilterHistoryBy sensorHistory.DeviceGroupId sensorHistory.PropertyId

        SensorHistoryCollection.ReplaceOneAsync<StorableSensorHistory>(
            filter,
            storableSensorHistory,
            BsonStorage.Replace
        )
        |> Async.AwaitTask
        |> Async.Ignore

    let Drop () =
        BsonStorage.Database.DropCollection(SensorHistoryCollectionName)
