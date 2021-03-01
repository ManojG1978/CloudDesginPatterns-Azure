using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;

namespace OrdersMaterializedViewCreator
{
    public class ViewProcessor
    {
        private DocumentClient _client;
        private Uri _collectionUri;
        private ILogger _log;

        private string _databaseName = Environment.GetEnvironmentVariable("DatabaseName");
        private string _collectionName = Environment.GetEnvironmentVariable("ViewCollectionName");


        public ViewProcessor(DocumentClient client, ILogger log)
        {
            _log = log;
            _client = client;
            _collectionUri = UriFactory.CreateDocumentCollectionUri(_databaseName, _collectionName);
        }

        public async Task UpdateOrderMaterializedView(OrderModel order)
        {
            _log.LogInformation("Updating materialized view");

            Document orderView = null;
            var requestOptions = new RequestOptions() { PartitionKey = new PartitionKey(order.OrderId) };

            int attempts = 0;

            while (attempts < 10)
            {
                try
                {
                    var documentUri = UriFactory.CreateDocumentUri(_databaseName, _collectionName, order.Id.ToString());

                    _log.LogInformation($"Materialized view: {documentUri}");

                    orderView = await _client.ReadDocumentAsync(documentUri, requestOptions);                
                }
                catch (DocumentClientException ex)
                {
                    if (ex.StatusCode != HttpStatusCode.NotFound)
                        throw;
                }

                orderView = OrderModel.ToDocument(order, orderView);

                AccessCondition acAll = new AccessCondition() {
                    Type = AccessConditionType.IfMatch,
                    Condition = orderView.ETag                
                };
                requestOptions.AccessCondition = acAll;
                
                try 
                {
                    await UpsertDocument(orderView, requestOptions);
                    return;
                }
                catch (DocumentClientException de) 
                {
                    if (de.StatusCode == HttpStatusCode.PreconditionFailed)
                    {
                        attempts += 1;
                        _log.LogWarning($"Optimistic concurrency pre-condition check failed. Trying again ({attempts}/10)");                        
                    }
                    else
                    {
                        throw;
                    }
                }              
            }

            throw new ApplicationException("Could not insert document after retrying 10 times, due to concurrency violations");
        }

        
        private async Task<ResourceResponse<Document>> UpsertDocument(object document, RequestOptions options)
        {
            int attempts = 0;

            while (attempts < 3)
            {
                try
                {
                    var result = await _client.UpsertDocumentAsync(_collectionUri, document, options);                      
                    _log.LogInformation($"{options.PartitionKey} RU Used: {result.RequestCharge:0.0}");
                    return result;                                  
                }
                catch (DocumentClientException de)
                {
                    if (de.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        _log.LogWarning($"Waiting for {de.RetryAfter} msec...");
                        await Task.Delay(de.RetryAfter);
                        attempts += 1;
                    }
                    else
                    {
                        throw;
                    }
                }
            }            

            throw new ApplicationException("Could not insert document after being throttled 3 times");
        }
    }
}
