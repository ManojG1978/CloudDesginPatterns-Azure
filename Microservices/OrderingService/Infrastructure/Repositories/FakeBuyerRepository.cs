using OrderingService.Domain.AggregatesModel.BuyerAggregate;
using OrderingService.Domain.SeedWork;
using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;

namespace OrderingService.Infrastructure.Repositories
{
    public class FakeBuyerRepository : IBuyerRepository, IUnitOfWork
    {
        private readonly IMediator _mediator;
        private readonly ILogger _logger;
        public IUnitOfWork UnitOfWork => this;
        private Buyer _buyer;

        public FakeBuyerRepository(IMediator mediator, ILoggerFactory loggerFactory)
        {
            _mediator = mediator;
            _logger = loggerFactory.CreateLogger(nameof(FakeBuyerRepository));
        }

        public Buyer Add(Buyer buyer)
        {
            _buyer = buyer;
            _logger.LogInformation($"Adding buyer entity with id: {buyer.Identity}.");
            return buyer;
        }

        public Buyer Update(Buyer buyer)
        {
            _buyer = buyer;
            _logger.LogInformation($"Updating buyer with id: {buyer.Identity}.");
            return buyer;
        }

        public Buyer Find(string buyerIdentityGuid)
        {
            _logger.LogInformation($"Finding buyer with id: {buyerIdentityGuid}");

            return new Buyer(buyerIdentityGuid, "Test");
        }

        public Buyer FindById(string id)
        {
            _logger.LogInformation($"Finding buyer with id: {id}");

            return new Buyer(id, "Test");
        }

        public void Dispose()
        {

        }

        public int SaveChanges()
        {
            _logger.LogInformation("Committing Buyer changes to the database");
            return 1;
        }

        public bool SaveEntities()
        {
            var domainEvents = _buyer.DomainEvents.ToList();

            _buyer.ClearDomainEvents();

            _logger.LogInformation("Publishing domain events and Committing buyer entities to the database");
            
            domainEvents.ForEach(async e => await _mediator.Publish(e));

            return true;
        }
    }
}
