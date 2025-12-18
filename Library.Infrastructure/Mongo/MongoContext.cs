
using MongoDB.Driver;

namespace Library.Infrastructure.Mongo
{
    public class MongoContext
    {
        public IMongoDatabase Database { get; }

        public MongoContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            Database = client.GetDatabase(databaseName);
        }

        //Creating Collections
        public void CreateCollectionsIfNotExist()
        {
            var existingCollections = Database.ListCollectionNames().ToList();

            if (!existingCollections.Contains("ExceptionLogs"))
                Database.CreateCollection("ExceptionLogs");

            if (!existingCollections.Contains("MessageLogs"))
                Database.CreateCollection("MessageLogs");
        }
    }
}
