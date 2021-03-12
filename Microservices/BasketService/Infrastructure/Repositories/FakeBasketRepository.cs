using System.Collections.Generic;
using BuildingBlocks;
using Microsoft.Extensions.Logging;
using CustomerBasket = BasketService.Domain.AggregateRoot.CustomerBasket;

namespace BasketService.Infrastructure.Repositories
{
    public class FakeBasketRepository : IBasketRepository
    {
        private readonly ILogger _logger;

        public FakeBasketRepository(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger(nameof(FakeBasketRepository));
        }
        public CustomerBasket GetBasket(string customerId)
        {
            _logger.LogInformation($"Getting basket for customer: {customerId}");
            return new CustomerBasket(customerId);
        }

        public IEnumerable<string> GetUsers()
        {
            return new List<string> {"1"};
        }

        public CustomerBasket UpdateBasket(CustomerBasket basket)
        {
            _logger.LogInformation($"Updating Basket with Buyer ID: ({basket.BuyerId})");
            return new CustomerBasket(basket.BuyerId);
        }

        public bool DeleteBasket(string id)
        {
            _logger.LogInformation($"Deleting basket for id: {id}");
            return true;
        }
    }
}
