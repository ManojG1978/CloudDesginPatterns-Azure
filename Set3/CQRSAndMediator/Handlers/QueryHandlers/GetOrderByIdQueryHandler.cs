using System.Threading;
using System.Threading.Tasks;
using CQRSAndMediator.Interfaces;
using CQRSAndMediator.RequestModels.QueryRequestModels;
using CQRSAndMediator.ResponseModels.QueryResponseModels;
using MediatR;

namespace CQRSAndMediator.Handlers.QueryHandlers
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdRequestModel, GetOrderByIdResponseModel>
    {
        private readonly ICosmosDbService _cosmosDbService;

        public GetOrderByIdQueryHandler(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }
        
        public async Task<GetOrderByIdResponseModel> Handle(GetOrderByIdRequestModel request, CancellationToken cancellationToken)
        {
            return await _cosmosDbService.GetItemAsync(request.OrderId);
        }
    }
}
