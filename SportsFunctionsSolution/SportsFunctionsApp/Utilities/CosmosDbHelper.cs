using Microsoft.Azure.Cosmos;

namespace SportsFunctionsApp.Utilities
{
    public class CosmosDbHelper
    {
        private readonly CosmosClient _cosmosClient;
        private readonly string _databaseId;

        public CosmosDbHelper(string endpointUri, string primaryKey, string databaseId)
        {
            _cosmosClient = new CosmosClient(endpointUri, primaryKey);
            _databaseId = databaseId;
        }

        public Container GetContainer(string containerId)
        {
            return _cosmosClient.GetContainer(_databaseId, containerId);
        }
    }
}
