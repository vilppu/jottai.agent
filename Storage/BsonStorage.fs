namespace Jottai

module BsonStorage = 
    open System
    open MongoDB.Driver
    open MongoDB.Bson.Serialization
    open MongoDB.Bson.Serialization.Conventions
    
    type IgnoreBackingFieldsConvention() =
        inherit ConventionBase()
        interface IMemberMapConvention with
            member this.Apply (memberMap : BsonMemberMap) =
                if (memberMap.MemberName.EndsWith "@")
                then memberMap.SetShouldSerializeMethod(fun o -> false) |> ignore

    let Database = 
        let client = MongoClient("mongodb://localhost/?maxPoolSize=1024")
        let databaseNameOrNull = Environment.GetEnvironmentVariable "JOTTAI_MONGODB_DATABASE"
        
        let databaseName = 
            match databaseNameOrNull with
            | null -> "Jottai"
            | databaseName -> databaseName

        let conventionPack = new ConventionPack()
        conventionPack.Add (IgnoreBackingFieldsConvention())
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
