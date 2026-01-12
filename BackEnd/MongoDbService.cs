using MongoDB.Driver;

public interface IMongoDbService
{
    IMongoCollection<T> GetCollection<T>(string collectionName);
    Task InsertDocumentAsync<T>(string collectionName, T document);
}

public class MongoDbService : IMongoDbService
{
    private readonly IMongoDatabase _database;

    public MongoDbService(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }

    public async Task InsertDocumentAsync<T>(string collectionName, T document)
    {
        var collection = _database.GetCollection<T>(collectionName);
        await collection.InsertOneAsync(document);
    }
}