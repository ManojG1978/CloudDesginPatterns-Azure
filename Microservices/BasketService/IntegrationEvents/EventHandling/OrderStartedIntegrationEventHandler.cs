using System;
using System.Threading.Tasks;
using BasketService.Infrastructure.Repositories;
using BuildingBlocks;
using BuildingBlocks.Abstractions;
using Microsoft.Extensions.Logging;

namespace BasketService.IntegrationEvents.EventHandling
{
    public class OrderStartedIntegrationEventHandler : ICanHandle<OrderStartedIntegrationEvent>
    {
        private readonly IBasketRepository _repository;
        private readonly ILogger _logger;

        public OrderStartedIntegrationEventHandler(IBasketRepository repository, ILoggerFactory loggerFactory)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = loggerFactory.CreateLogger(nameof(OrderStartedIntegrationEventHandler));
        }
        
        public async Task<bool> TryHandleAsync(OrderStartedIntegrationEvent @event)
        {
            _logger.LogInformation($"Handling the OrderStartedIntegrationEventHandler within Basket service for user id: {@event.UserId}");
            _repository.DeleteBasket(@event.UserId);
            return true;
        }
    }
}



