using System.Threading.Tasks;
using OrderingService.Domain.AggregatesModel.OrderAggregate;
using OrderingService.Domain.SeedWork;

namespace OrderingService.Infrastructure.Repositories
{
    //This is just the RepositoryContracts or Interface defined at the Domain Layer
    //as requisite for the Order Aggregate

    public interface IOrderRepository : IRepository<Order>
    {
        Order Add(Order order);

        void Update(Order order);

        Order Get(int orderId);
    }
}