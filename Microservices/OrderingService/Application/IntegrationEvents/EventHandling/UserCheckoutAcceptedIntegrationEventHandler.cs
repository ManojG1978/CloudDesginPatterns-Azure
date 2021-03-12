using System;
using System.Threading.Tasks;
using BuildingBlocks;
using BuildingBlocks.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderingService.Application.Commands;

namespace OrderingService.Application.IntegrationEvents.EventHandling
{
    public class UserCheckoutAcceptedIntegrationEventHandler 
        : ICanHandle<UserCheckoutAcceptedIntegrationEvent>
    {
        private readonly IMediator _mediator;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;
        private readonly ILogger _logger;

        public UserCheckoutAcceptedIntegrationEventHandler(IMediator mediator,
         IOrderingIntegrationEventService orderingIntegrationEventService, ILoggerFactory loggerFactory)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _orderingIntegrationEventService 
                = orderingIntegrationEventService ?? throw new ArgumentNullException(nameof(orderingIntegrationEventService));
            _logger = loggerFactory.CreateLogger((nameof(UserCheckoutAcceptedIntegrationEventHandler)));
            
        }

        
        public async Task<bool> TryHandleAsync(UserCheckoutAcceptedIntegrationEvent @event)
        {
            var result = false;

            // Send Integration event to clean basket once basket is converted to Order and before starting with the order creation process
            var orderStartedIntegrationEvent = new OrderStartedIntegrationEvent(@event.UserId);
            _orderingIntegrationEventService.PublishThroughEventBus(orderStartedIntegrationEvent);

            if (@event.RequestId != Guid.Empty)
            {
                var createOrderCommand = new CreateOrderCommand(@event.Basket.Items, @event.UserId, 
                    @event.UserName, @event.City, @event.Street, 
                    @event.State, @event.Country, @event.ZipCode,
                    @event.CardNumber, @event.CardHolderName, @event.CardExpiration,
                    @event.CardSecurityNumber, @event.CardTypeId);

                var requestCreateOrder = new IdentifiedCommand<CreateOrderCommand, bool>(createOrderCommand, @event.RequestId);
                try
                {
                    result = await _mediator.Send(requestCreateOrder);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex.ToString());
                }
                
            }            

            _logger.LogInformation(result ? 
                $"UserCheckoutAccepted integration event has been received and a create new order process is started with requestId: {@event.RequestId}" 
                : $"UserCheckoutAccepted integration event has been received but a new order process has failed with requestId: {@event.RequestId}");
            
            return true;
        }
    }
}