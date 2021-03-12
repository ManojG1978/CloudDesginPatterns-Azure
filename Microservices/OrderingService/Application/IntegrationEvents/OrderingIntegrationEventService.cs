using System;
using System.Threading.Tasks;
using BuildingBlocks.Abstractions;
using BuildingBlocks.Events;
using Microsoft.Extensions.Logging;

namespace OrderingService.Application.IntegrationEvents
{
    public class OrderingIntegrationEventService : IOrderingIntegrationEventService
    {
        private readonly IEventBus _eventBus;
        private readonly ILogger _logger;

        public OrderingIntegrationEventService(IEventBus eventBus, ILoggerFactory loggerFactory)
        {
            _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
            _logger = loggerFactory.CreateLogger(nameof(OrderingIntegrationEventService));
        }

        public void PublishThroughEventBus<T>(T evt) where T: IntegrationEvent
        {
            SaveEventAndOrderingContextChanges(evt);

            _logger.LogInformation($"Publishing event: {evt.GetType().Name} and marking event as published in the database");
            _eventBus.PublishAsync(evt);
           
            //Mark event as published
        }

        private void SaveEventAndOrderingContextChanges(IntegrationEvent evt)
        {
            //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
            //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency            

            //In one Transaction
            //Save Order
            //Save Event
            _logger.LogInformation($"Saving Order information and event within the same (resilient) transaction");
        }
    }
}
