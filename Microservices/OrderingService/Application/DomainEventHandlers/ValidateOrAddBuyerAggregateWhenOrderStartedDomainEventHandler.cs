using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderingService.Application.IntegrationEvents;
using OrderingService.Application.IntegrationEvents.Events;
using OrderingService.Domain.AggregatesModel.BuyerAggregate;
using OrderingService.Domain.Events;
using OrderingService.Infrastructure.Repositories;

namespace OrderingService.Application.DomainEventHandlers
{
    public class ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler 
                        : INotificationHandler<OrderStartedDomainEvent>
    {
        private readonly IBuyerRepository _buyerRepository;
        private readonly IOrderingIntegrationEventService _orderingIntegrationEventService;
        private readonly ILogger _logger;

        public ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler(
            IBuyerRepository buyerRepository, 
            IOrderingIntegrationEventService orderingIntegrationEventService,
            ILoggerFactory loggerFactory)
        {
            _buyerRepository = buyerRepository ?? throw new ArgumentNullException(nameof(buyerRepository));
            _orderingIntegrationEventService = orderingIntegrationEventService ?? 
                                               throw new ArgumentNullException(nameof(orderingIntegrationEventService));
            _logger = loggerFactory.CreateLogger(nameof(ValidateOrAddBuyerAggregateWhenOrderStartedDomainEventHandler));
        }

        public async Task Handle(OrderStartedDomainEvent orderStartedEvent, CancellationToken cancellationToken)
        {            
            var cardTypeId = (orderStartedEvent.CardTypeId != 0) ? orderStartedEvent.CardTypeId : 1;
            var buyer = _buyerRepository.Find(orderStartedEvent.UserId);
            bool buyerOriginallyExisted = (buyer != null);

            if (!buyerOriginallyExisted)
            {                
                buyer = new Buyer(orderStartedEvent.UserId, orderStartedEvent.UserName);
            }

            buyer.VerifyOrAddPaymentMethod(cardTypeId,
                                           $"Payment Method on {DateTime.UtcNow}",
                                           orderStartedEvent.CardNumber,
                                           orderStartedEvent.CardSecurityNumber,
                                           orderStartedEvent.CardHolderName,
                                           orderStartedEvent.CardExpiration,
                                           orderStartedEvent.Order.Id);

            var buyerUpdated = buyerOriginallyExisted ? 
                _buyerRepository.Update(buyer) : 
                _buyerRepository.Add(buyer);

            _buyerRepository.UnitOfWork.SaveEntities();

            //Not showing the implementation of this event handler in other aggregate roots
            var statusChangedToSubmittedIntegrationEvent 
                = new OrderStatusChangedToSubmittedIntegrationEvent(
                    orderStartedEvent.Order.Id, orderStartedEvent.Order.OrderStatus.Name, buyer.Name);
            
            _orderingIntegrationEventService.PublishThroughEventBus(statusChangedToSubmittedIntegrationEvent);

            _logger.LogInformation($"Buyer {buyerUpdated.Id} and related payment method were validated or updated for orderId: " +
                          $"{orderStartedEvent.Order.Id}.");
        }
    }
}
