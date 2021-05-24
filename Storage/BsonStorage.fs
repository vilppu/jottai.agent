namespace Jottai

module BsonStorage = 
    open System
    open System.Linq
    open MongoDB.Driver
    open MongoDB.Bson.Serialization
    open MongoDB.Bson.Serialization.Conventions
    open MongoDB.Bson.Serialization.Serializers
    
    type IgnoreBackingFieldsConvention() =
        inherit ConventionBase()
        interface IMemberMapConvention with
            member this.Apply (memberMap : BsonMemberMap) =
                if (memberMap.MemberName.EndsWith "@")
                then memberMap.SetShouldSerializeMethod(fun o -> false) |> ignore
    
    type DeviceGroupIdSerializer() =
        inherit SerializerBase<DeviceGroupId>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let (DeviceGroupId stringValue) = (value :?> DeviceGroupId)
                BsonSerializer.Serialize(context.Writer, stringValue :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let stringValue = BsonSerializer.Deserialize<string>(context.Reader)
                let deviceGroupId = DeviceGroupId stringValue                
                deviceGroupId :> obj
    
    type GatewayIdSerializer() =
        inherit SerializerBase<GatewayId>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let (GatewayId stringValue) = (value :?> GatewayId)
                BsonSerializer.Serialize(context.Writer, stringValue :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let stringValue = BsonSerializer.Deserialize<string>(context.Reader)
                let gatewayId = GatewayId stringValue                
                gatewayId :> obj
    
    type DeviceIdSerializer() =
        inherit SerializerBase<DeviceId>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let (DeviceId stringValue) = (value :?> DeviceId)
                BsonSerializer.Serialize(context.Writer, stringValue :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let stringValue = BsonSerializer.Deserialize<string>(context.Reader)
                let deviceId = DeviceId stringValue                
                deviceId :> obj
    
    type PropertyIdSerializer() =
        inherit SerializerBase<PropertyId>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let (PropertyId stringValue) = (value :?> PropertyId)
                BsonSerializer.Serialize(context.Writer, stringValue :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let stringValue = BsonSerializer.Deserialize<string>(context.Reader)
                let propertyId = PropertyId stringValue                
                propertyId :> obj
    
    type PropertyNameSerializer() =
        inherit SerializerBase<PropertyName>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let (PropertyName stringValue) = (value :?> PropertyName)
                BsonSerializer.Serialize(context.Writer, stringValue :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let stringValue = BsonSerializer.Deserialize<string>(context.Reader)
                let propertyName = PropertyName stringValue                
                propertyName :> obj
    
    type PropertyDescriptionSerializer() =
        inherit SerializerBase<PropertyDescription>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let (PropertyDescription stringValue) = (value :?> PropertyDescription)
                BsonSerializer.Serialize(context.Writer, stringValue :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let stringValue = BsonSerializer.Deserialize<string>(context.Reader)
                let propertyDescription = PropertyDescription stringValue                
                propertyDescription :> obj

    type UnwrapConvention() =
        inherit ConventionBase()
        interface IMemberMapConvention with
            member _.Apply (memberMap : BsonMemberMap) =
                if memberMap.MemberType = typeof<DeviceGroupId>
                then memberMap.SetSerializer(DeviceGroupIdSerializer()) |> ignore
                if memberMap.MemberType = typeof<GatewayId>
                then memberMap.SetSerializer(GatewayIdSerializer()) |> ignore
                if memberMap.MemberType = typeof<DeviceId>
                then memberMap.SetSerializer(DeviceIdSerializer()) |> ignore
                if memberMap.MemberType = typeof<PropertyId>
                then memberMap.SetSerializer(PropertyIdSerializer()) |> ignore
                if memberMap.MemberType = typeof<PropertyName>
                then memberMap.SetSerializer(PropertyNameSerializer()) |> ignore
                if memberMap.MemberType = typeof<PropertyDescription>
                then memberMap.SetSerializer(PropertyDescriptionSerializer()) |> ignore
    
    type DeviceProtocolSerializer() =
        inherit SerializerBase<DeviceProtocol>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let deviceProtocol = (value :?> DeviceProtocol)
                let stringValue = 
                   match deviceProtocol with
                   | ZWave -> "ZWave"
                   | ZWavePlus -> "ZWavePlus"
                   | _ -> "NotSpecified"
                BsonSerializer.Serialize(context.Writer, stringValue :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let stringValue = BsonSerializer.Deserialize<string>(context.Reader)
                let deviceProtocol = 
                   match stringValue with
                   | "ZWave" -> ZWave
                   | "ZWavePlus" -> ZWavePlus
                   | _ -> NotSpecified
                deviceProtocol :> obj

    type DeviceProtocolToStringConvention() =
        inherit ConventionBase()
        interface IMemberMapConvention with
            member _.Apply (memberMap : BsonMemberMap) =
                if memberMap.MemberType = typeof<DeviceProtocol>
                then memberMap.SetSerializer(DeviceProtocolSerializer()) |> ignore
    
    type ListSerializer<'T>() =
        inherit SerializerBase<List<'T>>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let list = (value :?> List<'T>)
                let serializableList = list.ToList<'T>()
                BsonSerializer.Serialize(context.Writer, serializableList :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let serializableList = BsonSerializer.Deserialize<Collections.Generic.List<'T>>(context.Reader)
                let list = serializableList |> Seq.toList                
                list :> obj

    type ListConvention() =
        inherit ConventionBase()
        interface IMemberMapConvention with
            member _.Apply (memberMap : BsonMemberMap) =                
                if memberMap.MemberType.IsGenericType
                then
                  let isList = memberMap.MemberType.GetGenericTypeDefinition() = typedefof<List<_>>
                  if isList
                  then
                    let typeArgument = memberMap.MemberType.GenericTypeArguments.[0]
                    let listSerializerType = typedefof<ListSerializer<_>>.MakeGenericType(typeArgument)
                    let listSerializer = System.Activator.CreateInstance listSerializerType :?> IBsonSerializer
                    memberMap.SetSerializer(listSerializer) |> ignore

    [<CLIMutable>]    
    type StorableMeasurement = 
        { mutable MeasuredProperty : string
          mutable MeasuredValue : obj }
    
    type MeasurementSerializer() =
        inherit SerializerBase<Measurement.Measurement>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let measurement = value :?> Measurement.Measurement
                let measuredProperty = measurement |> Measurement.Name
                let measuredValue = measurement |> Measurement.Value
                let storableMeasurement : StorableMeasurement =
                    { MeasuredProperty = measuredProperty
                      MeasuredValue = measuredValue }
                BsonSerializer.Serialize(context.Writer, storableMeasurement :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let storableMeasurement = BsonSerializer.Deserialize<StorableMeasurement>(context.Reader)
                let measurement = Measurement.From storableMeasurement.MeasuredProperty storableMeasurement.MeasuredValue
                match measurement with
                | Some measurement -> measurement :> obj
                | _ -> null

    type MeasurementToDocumentConvention() =
        inherit ConventionBase()
        interface IMemberMapConvention with
            member _.Apply (memberMap : BsonMemberMap) =
                if memberMap.MemberName = nameof(Measurement)
                then memberMap.SetSerializer(MeasurementSerializer()) |> ignore

    [<CLIMutable>]    
    type StorablePropertyValue = 
        { mutable PropertyType : string
          mutable PropertyValue : obj }
    
    type PropertySerializer() =
        inherit SerializerBase<Measurement.Measurement>()
        interface IBsonSerializer with
            member this.Serialize (context : BsonSerializationContext, args : BsonSerializationArgs, value : obj) =
                let deviceProperty = value :?> DeviceProperty.DeviceProperty
                let propertyType = deviceProperty |> DeviceProperty.Name
                let propertyValue = deviceProperty |> DeviceProperty.Value
                let storablePropertyValue : StorablePropertyValue =
                    { PropertyType = propertyType
                      PropertyValue = propertyValue }
                BsonSerializer.Serialize(context.Writer, storablePropertyValue :> obj)

            member this.Deserialize (context : BsonDeserializationContext, args : BsonDeserializationArgs) =
                let storablePropertyValue = BsonSerializer.Deserialize<StorablePropertyValue>(context.Reader)
                let measurement = DeviceProperty.From storablePropertyValue.PropertyType storablePropertyValue.PropertyValue
                match measurement with
                | Some measurement -> measurement :> obj
                | _ -> null

    type DevicePropertyToDocumentConvention() =
        inherit ConventionBase()
        interface IMemberMapConvention with
            member _.Apply (memberMap : BsonMemberMap) =
                if memberMap.MemberName = nameof(DeviceProperty)
                then memberMap.SetSerializer(PropertySerializer()) |> ignore

    let Database = 
        let client = MongoClient("mongodb://localhost/?maxPoolSize=1024")
        let databaseNameOrNull = Environment.GetEnvironmentVariable "JOTTAI_MONGODB_DATABASE"
        
        let databaseName = 
            match databaseNameOrNull with
            | null -> "Jottai"
            | databaseName -> databaseName

        let conventionPack = new ConventionPack()
        conventionPack.Add (IgnoreExtraElementsConvention(true))
        conventionPack.Add (IgnoreBackingFieldsConvention())
        conventionPack.Add (UnwrapConvention())
        conventionPack.Add (MeasurementToDocumentConvention())
        conventionPack.Add (DeviceProtocolToStringConvention())
        conventionPack.Add (ListConvention())
        ConventionRegistry.Register("JottaiConventions", conventionPack, (fun t -> true));

        client.GetDatabase databaseName
    
    let DropCollection(collection : IMongoCollection<'T>) = 
        let collectionName = collection.CollectionNamespace.CollectionName
        let database = collection.Database
        database.DropCollection(collectionName)
    
    let WithDescendingIndex<'TDocument> fieldName (collection : IMongoCollection<'TDocument>) = 
        let builder = Builders<'TDocument>.IndexKeys
        let field = FieldDefinition<'TDocument>.op_Implicit(fieldName)
        let key = builder.Descending(field)
        let index = new CreateIndexModel<'TDocument>(key)
        collection.Indexes.CreateOne index |> ignore
        collection       
        
    let Upsert =
        let options = UpdateOptions()
        options.IsUpsert <- true
        options

    let Replace =
        let options = ReplaceOptions()
        options.IsUpsert <- true
        options
