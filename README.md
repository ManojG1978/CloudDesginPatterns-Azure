# Cloud Patterns in Azure

The lab is divided into the following sets of exercises. Each set covers a group of related patterns demonstrated through a simple application written in .NET Core/ASP.NET Core and deployed to an Azure service as applicable

## Set 1: Circuit Breaker and Health Monitoring

1) This set includes two patterns:
   1) [Circuit Breaker](https://docs.microsoft.com/en-us/azure/architecture/patterns/circuit-breaker)
   2) [Health Endpoint Monitoring](https://docs.microsoft.com/en-us/azure/architecture/patterns/health-endpoint-monitoring)
2) The Circuit Breaker Pattern is implemented using a popular package called [Polly](https://github.com/App-vNext/Polly), which also provides a framework for other patterns like [Retry](https://docs.microsoft.com/en-us/azure/architecture/patterns/retry)
3) Health Monitoring is built using the standard ASP.NET Core extension *Microsoft.AspNetCore.Diagnostics.HealthChecks*
4) The interesting aspect about this pattern is how Health Monitoring and Circuit Breaker can be integrated. The working sample can be found in the Set1 folder. The snippet of code of how this works (with very little code) is shown below.
5) In this implementation, the Circuit Breaker automatically switches to an Open State after two failed requests. The circuit is open for 30 seconds and a successful request comes through, the the circuit status switches to Closed. These state transitions also automatically switch the state of the Health Monitoring to Unhealthy and Healthy respectively.

```csharp

 var basicCircuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30), OnBreak, OnReset, OnHalfOpen);

services.AddHttpClient<ITemperatureService, TemperatureService>("TemperatureService").AddPolicyHandler(basicCircuitBreakerPolicy);
            
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
