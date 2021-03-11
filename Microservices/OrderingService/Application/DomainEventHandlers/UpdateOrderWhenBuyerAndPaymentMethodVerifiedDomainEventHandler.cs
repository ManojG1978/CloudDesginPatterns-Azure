using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderingService.Domain.Events;
using OrderingService.Infrastructure.Repositories;

namespace OrderingService.Application.DomainEventHandlers
{
    public class UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler 
                   : INotificationHandler<BuyerAndPaymentMethodVerifiedDomainEvent>
    {
        private readonly IOrderRepository _orderRepository;        
        private readonly ILogger _logger;        

        public UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler(
            IOrderRepository orderRepository, ILoggerFactory loggerFactory)            
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));            
            var factory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = factory.CreateLogger(nameof(UpdateOrderWhenBuyerAndPaymentMethodVerifiedDomainEventHandler));
        }

        // Domain Logic comment:
        // When the Buyer and Buyer's payment method have been created or verified that they existed, 
        // then we can update the original Order with the BuyerId and PaymentId (foreign keys)
        public async Task Handle(BuyerAndPaymentMethodVerifiedDomainEvent buyerPaymentMethodVerifiedEvent, 
            CancellationToken cancellationToken)
        {
            var orderToUpdate = _orderRepository.Get(buyerPaymentMethodVerifiedEvent.OrderId);
            orderToUpdate.SetBuyerId(buyerPaymentMethodVerifiedEvent.Buyer.Id);
            orderToUpdate.SetPaymentId(buyerPaymentMethodVerifiedEvent.Payment.Id);                                                

            _logger.LogInformation($"Order with Id: {buyerPaymentMethodVerifiedEvent.OrderId} has been successfully updated " +
                                   $"with a payment method id: { buyerPaymentMethodVerifiedEvent.Payment.Id }");                        
        }
    }  
}
