using System.Collections.Generic;
using System.Linq;
using BuildingBlocks;
using OrderingService.Application.Commands;

namespace OrderingService.Application
{
    public static class BasketItemExtensions
    {
        public static IEnumerable<CreateOrderCommand.OrderItem> ToOrderItems(this IEnumerable<BasketItem> basketItems)
        {
            return basketItems.Select(item => item.ToOrderItem());
        }

        public static CreateOrderCommand.OrderItem ToOrderItem(this BasketItem item)
        {
            return new CreateOrderCommand.OrderItem()
            {
                ProductId = int.TryParse(item.ProductId, out int id) ? id : -1,
                ProductName = item.ProductName,
                PictureUrl = item.PictureUrl,
                UnitPrice = item.UnitPrice,
                Units = item.Quantity
            };
        }
    }
}
