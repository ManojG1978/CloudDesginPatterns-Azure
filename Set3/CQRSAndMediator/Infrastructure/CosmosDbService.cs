using System.Linq;
using System.Threading.Tasks;
using CQRSAndMediator.Interfaces;
using CQRSAndMediator.RequestModels.CommandRequestModels;
using CQRSAndMediator.ResponseModels.QueryResponseModels;
using Microsoft.Azure.Cosmos;

namespace CQRSAndMediator.Infrastructure
{
    public class CosmosDbService : ICosmosDbService
    {
        private readonly CosmosClient _dbClient;
        private readonly string _databaseName;
        private readonly string _readContainerName;
        private readonly string _writeContainerName;

        public CosmosDbService(
            CosmosClient dbClient,
            string databaseName,
            string readContainerName,
            string writeContainerName)
        {
            _dbClient = dbClient;
            _databaseName = databaseName;
            _readContainerName = readContainerName;
            _writeContainerName = writeContainerName;
        }

        public async Task AddOrderAsync(MakeOrderRequest order)
        {
            var container = _dbClient.GetContainer(_databaseName, _writeContainerName);
            await container.CreateItemAsync(order, new PartitionKey(order.OrderId));
        }


        public async Task<GetOrderByIdResponseModel> GetItemAsync(int orderId)
        {
            try
            {
                var container = _dbClient.GetContainer(_databaseName, _readContainerName);

                QueryDefinition query = new QueryDefinition(
                        "select * from orders where orders.orderId = @orderId")
                    .WithParameter("@orderId", orderId);


                using (FeedIterator<GetOrderByIdResponseModel> resultSet =
                    container.GetItemQueryIterator<GetOrderByIdResponseModel>(
                        query,
                        requestOptions: new QueryRequestOptions()
                        {
                            PartitionKey = new PartitionKey(orderId),
                            MaxItemCount = 1
                        }))
                {
                    GetOrderByIdResponseModel order = null;
                    while (resultSet.HasMoreResults)
                    {
                        FeedResponse<GetOrderByIdResponseModel> response = await resultSet.ReadNextAsync();
                        order = response.FirstOrDefault();
                        break;
                    }

                    return order;
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }
    }
}