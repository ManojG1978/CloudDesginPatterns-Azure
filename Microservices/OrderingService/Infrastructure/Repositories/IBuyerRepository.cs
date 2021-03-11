using OrderingService.Domain.AggregatesModel.BuyerAggregate;
using OrderingService.Domain.SeedWork;

namespace OrderingService.Infrastructure.Repositories
{
    //This is just the RepositoryContracts or Interface defined at the Domain Layer
    //as requisite for the Buyer Aggregate

    public interface IBuyerRepository : IRepository<Buyer>
    {
        Buyer Add(Buyer buyer);
        Buyer Update(Buyer buyer);
        Buyer Find(string buyerIdentityGuid);
        Buyer FindById(string id);
    }
}