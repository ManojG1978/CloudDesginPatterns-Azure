using System;
using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using OrderingService.Domain.AggregatesModel.BuyerAggregate;
using OrderingService.Domain.AggregatesModel.OrderAggregate;
using OrderingService.Domain.SeedWork;

namespace OrderingService.Infrastructure.Repositories
{
    public class FakeOrderRepository : IOrderRepository, IUnitOfWork
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        public IUnitOfWork UnitOfWork => this;
        private Order _order;

        public FakeOrderRepository(IMediator mediator, ILoggerFactory loggerFactory)
        {
            _mediator = mediator;
            _logger = loggerFactory.CreateLogger(nameof(FakeOrderRepository));
        }

        public Order Add(Order order)
        {
            _order = order;
            _logger.LogInformation($"Adding an order entity[{order.Id}]");
            return order;
        }

        public void Update(Order order)
        {
            _order = order;
            _logger.LogInformation($"Updating an order entity[{order.Id}]");

        }

        public Order Get(int orderId)
        {
            return new  Order(Guid.NewGuid().ToString(), "Test", 
                new Address("street", "city", "state", "country", "00001"), 
                CardType.Amex.Id, "11111111","000", "Test", new DateTime(2018,12,12), 1);
        }

        public void Dispose()
        {
        }

        public int SaveChanges()
        {
            _logger.LogInformation("Committing Order aggregate to the database");
            return 1;
        }

        public bool SaveEntities()
        {
            var domainEvents = _order.DomainEvents.ToList();

            _order.ClearDomainEvents();

            _logger.LogInformation($"Publishing domain events and committing Order entities to the database");
            
            domainEvents.ForEach(async e => await _mediator.Publish(e));

            return true;
        }
    }
}
