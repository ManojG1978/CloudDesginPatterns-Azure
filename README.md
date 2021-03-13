# Cloud Patterns in Azure

The lab is divided into the following sets of exercises. Each set covers a group of related patterns demonstrated through a simple application written in .NET Core/ASP.NET Core and deployed to an Azure service as applicable

## Lab 1: Circuit Breaker, Retry & Health Monitoring

| Problem Statement | Solution | Alternatives/Related Patterns
| ----------------- | ---------|-----------------------------|
|You are an architect of a company that provides Weather APIs. You rely on a large number of external partner APIs which have a history of intermittent failures and reliability issues. You want to ensure these issues do not adversely affect the stability and experience of your services. | You implement the [Circuit Breaker](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker) Pattern using the Polly package to detect repeated failures on the 3rd party service and temporarily disable the endpoint. This way the dependent service can fallback gracefully. The Circuit breaker works in concert with the [Health Endpoint Monitoring](https://docs.microsoft.com/en-us/azure/architecture/patterns/health-endpoint-monitoring) pattern - a dedicated URL exposed by the endpoint to check on the health and liveness periodically. This is done using the ASP.NET Core built-in health checks framework.| The [Retry](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry) pattern can be implemented to retry requests on a 3rd party service which has intermittent/transient failures and back-off after repeated failures (supported by Polly)|

1) The Circuit Breaker Pattern is implemented using a popular package called [Polly](https://github.com/App-vNext/Polly), which also provides a framework for other patterns like Retry
2) Health Monitoring is built using the standard ASP.NET Core extension *Microsoft.AspNetCore.Diagnostics.HealthChecks*
3) The interesting aspect about this pattern is how Health Monitoring and Circuit Breaker can be integrated. The working sample can be found in the Set1 folder. The snippet of code of how this works (with very little code) is shown below.
4) In this implementation, the Circuit Breaker automatically switches to an Open State after two failed requests. The circuit is open for 30 seconds and a successful request comes through, the the circuit status switches to Closed. These state transitions also automatically switch the state of the Health Monitoring to Unhealthy and Healthy respectively.
5) A Transient Http Error Policy is also added to the Http Client, which automatically adds a retry should a request fail. Subsequent calls will be made in 1, 5 and 10 seconds after failure.

```csharp
 var basicCircuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30), OnBreak, OnReset, OnHalfOpen);

services.AddHttpClient<ITemperatureService, TemperatureService>("TemperatureService")
                .AddPolicyHandler(basicCircuitBreakerPolicy)
                .AddTransientHttpErrorPolicy(builder =>
                    builder.WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10)
                    }));     

services.AddHealthChecks()
    .AddCheck("Temperature Service", () =>
    {
        return basicCircuitBreakerPolicy.CircuitState switch
        {
            CircuitState.Open => HealthCheckResult.Unhealthy(),
            CircuitState.HalfOpen => HealthCheckResult.Degraded(),
            _ => HealthCheckResult.Healthy()
        };
    });

services.AddHealthChecksUI((settings =>
{
    settings.AddHealthCheckEndpoint("Weather Service", "/hc");
})).AddInMemoryStorage();     
```

## Lab 2: Cache Aside

| Problem Statement | Solution | Alternatives/Related Patterns
| ----------------- | ---------|-----------------------------|
|You are an architect of a company that provides Weather APIs. You want to improve the performance (response time) of your API by caching results of your temperature API (for which you rely on a partner). | You use the [Cache Aside](https://docs.microsoft.com/en-us/azure/architecture/patterns/cache-aside) pattern to cache responses based on a policy. You use Polly's Cache policy for the HttpClient with an InMemoryCache provider (optionally, a distributed cache with Azure Redis Cache when deployed in a cluster).| |

1) The sample code is in the Set2 folder.
2) The following code is added in the *ConfigureServices* method, where an in-memory cache is configured. (On similar lines, you could configure a distributed cache with Azure Redis):

```csharp
var policyRegistry = AddInMemoryCache(services);

services.AddHttpClient<ITemperatureService, TemperatureService>("TemperatureService")
    .AddPolicyHandlerFromRegistry((pairs, message) => 
        policyRegistry.Get<IAsyncPolicy<HttpResponseMessage>>("CachingPolicy"));
```

3) In the *Configure* method, you setup the caching policy. In this particular case, cache expires after 30 seconds

```csharp

 registry.Add("CachingPolicy", Policy.CacheAsync<HttpResponseMessage>(cacheProvider, TimeSpan.FromSeconds(30)));

```

4) The sample leverages Polly's Caching policy to cache Http Responses for a given context. Caching is implemented for the Temperature (*GetCelcius*) service, with a key for every location passed to the Service.

```csharp
public async Task<HttpResponseMessage> GetCelsius(int locationId)
{
    var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get,
        new Uri(_client.BaseAddress + $"celsius/{locationId}"));

    httpRequestMessage.SetPolicyExecutionContext(new Context($"GetCelsius-{locationId}"));

    return await _client.SendAsync(httpRequestMessage);
}
```

4) When you run the application (F5 in Visual Studio), you can see that Temperature service is called the first time and not called for the next 30 seconds (The response is cached in memory).

## Lab 3: Gateway Offloading, Throttling, Gateway Routing, Ambassador

| Problem Statement | Solution | Alternatives/Related Patterns
| ----------------- | ---------|-----------------------------|
| You are an architect of a company that provides Weather APIs. You want to add resilience to your APIs by adding features like throttling. You also want the flexibility of supporting multiple concurrent versions for your backend APIs. You rely on a 3rd party for the Temperature API, which is considered legacy. You want to decouple your technology implementation from this and stick to modern protocols and message formats | You employ the [Gateway Offloading](https://docs.microsoft.com/en-us/azure/architecture/patterns/gateway-offloading) and [Throttling](https://docs.microsoft.com/en-us/azure/architecture/patterns/throttling) patterns by leveraging Azure API management  service- using it to wrap your API and configure appropriate rules. You employ the [Ambassador](https://docs.microsoft.com/en-us/azure/architecture/patterns/ambassador) patterns to wrap the legacy API and gives it a modern façade by employing appropriate transformation policies | Additionally, [Gateway Routing](https://docs.microsoft.com/en-us/azure/architecture/patterns/gateway-routing) pattern can be implemented by configuring routing rules to connect to different micro service backends using a single endpoint (differentiated by URL paths). |

1) In this lab, we are going to leverage Azure API management Service (PaaS) and add the weather API backends to it.
2) We can leverage the Temperature and Weather services created in Lab 2 as backend. Publish each of those to Azure app services. Navigate to each of the API endpoints and get the Open API/Swagger Spec (something like https://yourtemperatureservice.azurewebsites.net/swagger/v1/swagger.json).
3) Create a new API management service instance in the Basic tier (consumption tier does not support throttling)
4) Using the APIs specs from step #2, create two API backends from the Swagger JSON files.
5) While creating the API backends you can chose a different URL suffix for each service (like https://yourweatherservice-apim.azure-api.net/temperature/Celsius/{locationId}). So a single API management endpoint routes traffic to each of the backends based on URL paths
6) Consider any API and add an inbound policy to accommodate rate limiting like below:

```xml
<rate-limit-by-key  calls="10"
          renewal-period="60"
          counter-key="@(context.Request.IpAddress)" />

```

## Lab 4: Competing Consumer, Queue based load levelling, Pipes and Filters

| Problem Statement | Solution | Alternatives/Related Patterns
| ----------------- | ---------|-----------------------------|
| You are building a solution for COVID front-line workers. You want to build a scalable solution for transcribing audio report files uploaded by front line workers. Also, you want to support a background process which performs text analytics on the transcribed data you get from the first step, but you want to do this without affecting the performance of your end-user facing systems.| You implement the [Competing Consumer](https://docs.microsoft.com/en-us/azure/architecture/patterns/competing-consumers) pattern with Azure Functions and a Blob Trigger. The function does the work of transcribing with another blob configured as the output binding. This in turn, triggers a series of  text analytics tasks (like Entity Recognition etc.) which are invoked by different Azure functions in parallel. The transcribing and the text analytics tasks are decoupled by leveraging the [Queue based load levelling](https://docs.microsoft.com/en-us/azure/architecture/patterns/queue-based-load-leveling) pattern | A slight variant is the use of the [Pipes & Filters](https://docs.microsoft.com/en-us/azure/architecture/patterns/pipes-and-filters) pattern, where you use an Azure Queue based pipeline and a series of Azure Functions which perform different tasks on the same message, enriching it along the way |

1) Create an Azure Function App (consumption plan) and deploy two functions *PerformTextAnalytics* and *SoundFileProcessor*
2) The first stage goes through the *SoundFileProcessor* function which is trigger as messages are dropped into *covid-voice-files* Azure Storage queue. This function simulates sound file transcription and writes out a message in the *transcribed-voice-files* queue.
3) The second stage of the processing is to enrich the transcribed message with some text analytics. This is written as another Azure function *PerformTextAnalytics* which is triggered when queue messages are dropped into *transcribed-voice-files*. This function simulates the process of performing text analytics and the enriched message is dropped into the *processed-voice-files* Azure storage queue.  
4) Use the *QueueMessageGenerator* console app to generate a given number of messages to the *covid-voice-files* Azure storage queue. This triggers the pipeline of processing explained in the steps before.

## Lab 5: CQRS, Materialized View, Event Sourcing

| Problem Statement | Solution | Alternatives/Related Patterns
| ----------------- | ---------|-----------------------------|
| You are the architect in an ecommerce company, where the response times for the end user application is critical. While the application sees large volumes of order created by users, you want to allow users to search their order data and use different type of queries. You don't want the query load of the application to affect the transaction processing performance of the orders | You employ the [CQRS](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs) pattern to segregate the  order query processing logic and data source and the order creation logic and its data source (represented as two separate Cosmos DB containers). MediatR is used to decouple the command and query processing logic from the consuming service. They are deployed and scaled independently without impacting one other. On the data storage a separate [Materialized View](https://docs.microsoft.com/en-us/azure/architecture/patterns/materialized-view) is created just to support querying in another Cosmos DB container. The materialized view is updated using the Change feed feature of Cosmos DB, leveraging an Azure Function |The Change feed feature also supports the [Event Sourcing](https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing) pattern where specific change events can be consumed and replayed |

1) The samples are provided in the Set 3 folder.
2) Create a CosmosDB database with two containers, one that will serve data for querying, and one that will serve requests for any update or create operations.
3) The *CQRSAndMediator* project has a simple API controller (*OrderController*) which uses MediatR to send separate commands for creating an order and querying an order from their respective containers.

```csharp
public class MakeOrderCommandHandler: IRequestHandler<MakeOrderRequestModel , MakeOrderResponseModel>
    {
        private readonly ICosmosDbService _cosmosDbService;

        public MakeOrderCommandHandler(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }
        
        public async Task<MakeOrderResponseModel> Handle(MakeOrderRequestModel request, CancellationToken cancellationToken)
        {
            request.Id = Guid.NewGuid();
            await _cosmosDbService.AddOrderAsync(request);
            
            var result = new MakeOrderResponseModel
            {
                IsSuccess = true,
                OrderId = request.OrderId
            };
            return result;
        }
    }

```
```csharp
public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdRequestModel, GetOrderByIdResponseModel>
    {
        private readonly ICosmosDbService _cosmosDbService;

        public GetOrderByIdQueryHandler(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }
        
        public async Task<GetOrderByIdResponseModel> Handle(GetOrderByIdRequestModel request, CancellationToken cancellationToken)
        {
            return await _cosmosDbService.GetItemAsync(request.OrderId);
        }
    }

```

4) The *OrdersMaterializedViewCreator* project can be deployed to an Azure Function App, which will respond to the change feed on the order CosmosDB container. The changes are then written to the *ordersmv* container

```csharp
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
                var p = new ViewProcessor(client, log);
                
                log.LogInformation($"Processing {input.Count} events");
                
                foreach(var d in input)
                {
                    var order = OrderModel.FromDocument(d);
                    await p.UpdateOrderMaterializedView(order);

                }    
            }
        }
    }
```

## Lab 6: Valet Key Pattern

| Problem Statement | Solution | Alternatives/Related Patterns
| ----------------- | ---------|-----------------------------|
| You are the architect of an application which allows users to download all their data on request. Once a request is made by the user, the application puts together all information in a secure location and shares the link in a secure way. The application should be offloading that responsibility entirely as the data may be very large. Also users should just have enough permission to read/download the data. This link should be expire in a given period of time (1 Week by default)| You use the [Valet Key](https://docs.microsoft.com/en-us/previous-versions/msp-n-p/dn568102(v=pandp.10)) Pattern with Azure Blobs. You generate Shared Access Signatures (SAS) for each user container and share that from the app. The user can use the SAS URI and access the contents with a suitable tool|

1) The Set5\ValetKey project has a simple console application that simulates the end user scenario of create a blob container, uploading some sample file and then generating a SAS URI, which is then used to download and print the contents of the file.

## Lab 7: Domain Driven Design for a Microservices approach

1) The Microservices folder has a C# solution with projects representing a small subset of the full [eShop](https://github.com/dotnet-architecture/eShopOnContainers) reference implementation from Microsoft.
2) The sample simulates the set of processes that are triggered on a ecommerce site after the user decides to checkout the order.
3) The solution does not have any database (only fake repositories are used) and employs a simple in-memory event bus for domain integration events used to communicate between different microservices (Order and Basket in this example)
4) Run the solution to check the flow of events and the sequence of processing. The solution uses MediaTr for command handling
5) The solution also showcases a typical solution structure that can be used in a typical microservices project. 
