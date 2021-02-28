using System;
using System.Threading;
using System.Threading.Tasks;
using CQRSAndMediator.Interfaces;
using CQRSAndMediator.RequestModels.CommandRequestModels;
using CQRSAndMediator.ResponseModels.CommandResponseModels;
using MediatR;

namespace CQRSAndMediator.Handlers.CommandHandlers
{
    public class MakeOrderCommandHandler: IRequestHandler<MakeOrderRequestModel , MakeOrderResponseModel>
    {
        private readonly ICosmosDbService _cosmosDbService;

        public MakeOrderCommandHandler(ICosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }
        
        public async Task<MakeOrderResponseModel> Handle(MakeOrderRequestModel request, CancellationToken cancellationToken)
        {
            request.Id = Guid.NewGuid();
            await _cosmosDbService.AddOrderAsync(request);
            
            var result = new MakeOrderResponseModel
            {
                IsSuccess = true,
                OrderId = request.OrderId
            };
            return result;
        }
    }
}
