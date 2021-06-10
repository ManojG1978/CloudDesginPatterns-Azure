using System.Threading.Tasks;
using CQRSAndMediator.RequestModels.CommandRequestModels;
using CQRSAndMediator.ResponseModels.QueryResponseModels;

namespace CQRSAndMediator.Interfaces
{
    public interface ICosmosDbService
    {
        Task AddOrderAsync(MakeOrderRequest order);
        Task<GetOrderByIdResponseModel> GetItemAsync(int orderId);
    }
}
