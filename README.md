# Cloud Patterns in Azure

The lab is divided into the following sets of exercises. Each set covers a group of related patterns demonstrated through a simple application written in .NET Core/ASP.NET Core and deployed to an Azure service as applicable

## Lab 1: Circuit Breaker, Retry & Health Monitoring

1) This set includes two patterns:
   1) [Circuit Breaker](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
   2) [Health Endpoint Monitoring](https://docs.microsoft.com/en-us/azure/architecture/patterns/health-endpoint-monitoring)
2) The Circuit Breaker Pattern is implemented using a popular package called [Polly](https://github.com/App-vNext/Polly), which also provides a framework for other patterns like [Retry](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry)
3) Health Monitoring is built using the standard ASP.NET Core extension *Microsoft.AspNetCore.Diagnostics.HealthChecks*
4) The interesting aspect about this pattern is how Health Monitoring and Circuit Breaker can be integrated. The working sample can be found in the Set1 folder. The snippet of code of how this works (with very little code) is shown below.
5) In this implementation, the Circuit Breaker automatically switches to an Open State after two failed requests. The circuit is open for 30 seconds and a successful request comes through, the the circuit status switches to Closed. These state transitions also automatically switch the state of the Health Monitoring to Unhealthy and Healthy respectively.
6) A Transient Http Error Policy is also added to the Http Client, which automatically adds a retry should a request fail. Subsequent calls will be made in 1, 5 and 10 seconds after failure.

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

1) The sample demonstrates [Cache Aside](https://docs.microsoft.com/en-us/azure/architecture/patterns/cache-aside) pattern. 
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

This set includes the following patterns:
    1) [Gateway Offloading](https://docs.microsoft.com/en-us/azure/architecture/patterns/gateway-offloading)
    2) [Throttling](https://docs.microsoft.com/en-us/azure/architecture/patterns/throttling)
    3) [Gateway Routing](https://docs.microsoft.com/en-us/azure/architecture/patterns/gateway-routing)
    4) [Ambassador](https://docs.microsoft.com/en-us/azure/architecture/patterns/ambassador)


## Lab 4: Competing Consumer, Queue based load levelling, Pipes and Filters

This set includes the following patterns:
   1) [Competing Consumer](https://docs.microsoft.com/en-us/azure/architecture/patterns/competing-consumers)
   2) [Pipes & Filters](https://docs.microsoft.com/en-us/azure/architecture/patterns/pipes-and-filters)
   3) [Queue based load levelling](https://docs.microsoft.com/en-us/azure/architecture/patterns/queue-based-load-leveling)

## Lab 5: CQRS, Materialized View, Event Sourcing

This set includes the following patterns:
   1) [CQRS](https://docs.microsoft.com/en-us/azure/architecture/patterns/cqrs)
   2) [Materialized View](https://docs.microsoft.com/en-us/azure/architecture/patterns/materialized-view)
   3) [Event Sourcing](https://docs.microsoft.com/en-us/azure/architecture/patterns/event-sourcing)

