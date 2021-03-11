using BuildingBlocks.Events;

namespace OrderingService.Application.IntegrationEvents.Events
{
    public class OrderStatusChangedToSubmittedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get; private set; }
        public string OrderStatus { get; private set; }
        public string BuyerName { get; private set; }

        public OrderStatusChangedToSubmittedIntegrationEvent(int orderId, string orderStatus, string buyerName)
        {
            OrderId = orderId;
            OrderStatus = orderStatus;
            BuyerName = buyerName;
        }
    }
}
