using System;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
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
            var order = new Faker<MakeOrderRequest>()
                .RuleFor(o=> o.Amount, f=> f.Random.Int(10, 100))
                .RuleFor(o => o.Quantity, request.Quantity)
                .RuleFor(o => o.Id, Guid.NewGuid)
                .RuleFor(o => o.OrderedBy, request.OrderedBy)
                .RuleFor(o => o.ProductName, request.ProductName)
                .RuleFor(o => o.ProductId, Guid.NewGuid)
                .RuleFor(o => o.OrderedById, Guid.NewGuid)
                .RuleFor(o => o.OrderId, f=>f.Random.Int(1000, 1000000))
                .Generate();

            await _cosmosDbService.AddOrderAsync(order);
            
            var result = new MakeOrderResponseModel
            {
                IsSuccess = true,
                OrderId = order.OrderId            };
            return result;
        }
    }
}
