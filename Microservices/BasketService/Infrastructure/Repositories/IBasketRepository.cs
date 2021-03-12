using System.Collections.Generic;
using BuildingBlocks;
using CustomerBasket = BasketService.Domain.AggregateRoot.CustomerBasket;

namespace BasketService.Infrastructure.Repositories
{
    public interface IBasketRepository
    {
        CustomerBasket GetBasket(string customerId);
        IEnumerable<string> GetUsers();
        CustomerBasket UpdateBasket(CustomerBasket basket);
        bool DeleteBasket(string id);
    }
}
