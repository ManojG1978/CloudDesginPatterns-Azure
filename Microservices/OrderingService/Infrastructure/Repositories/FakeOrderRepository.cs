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
            _logger.LogInformation($"Adding an order[{order.Id}]. Not committing yet");
            return order;
        }

        public void Update(Order order)
        {
            _order = order;
            _logger.LogInformation($"Updating an order[{order.Id}]. Not committing yet");

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
            _logger.LogInformation("Committing Order and related changes to the database");
            return 1;
        }

        public bool SaveEntities()
        {
            var domainEvents = _order.DomainEvents.ToList();

            _order.ClearDomainEvents();

            domainEvents.ForEach(async e => await _mediator.Publish(e));

            _logger.LogInformation("Publishing events and committing Order entities to the database");

            return true;
        }
    }
}
