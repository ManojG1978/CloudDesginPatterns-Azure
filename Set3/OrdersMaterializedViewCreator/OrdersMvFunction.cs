using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace OrdersMaterializedViewCreator
{
    public static class OrdersMvFunction
    {
        [FunctionName("OrdersMaterializedViewCreator")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "%DatabaseName%",
            collectionName: "%SourceCollectionName%",
            ConnectionStringSetting = "ordersDbConnection",
            LeaseCollectionName = "leases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input,
            [CosmosDB(
                databaseName: "%DatabaseName%",
                collectionName: "%ViewCollectionName%",
                ConnectionStringSetting = "ordersDbConnection"
            )]DocumentClient client,
            ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                var viewProcessor = new ViewProcessor(client, log);
                
                log.LogInformation($"Processing {input.Count} events");
                
                foreach(var d in input)
                {
                    var order = OrderModel.FromDocument(d);
                    await viewProcessor.UpdateOrderMaterializedView(order);

                }    
            }
        }
    }
}
