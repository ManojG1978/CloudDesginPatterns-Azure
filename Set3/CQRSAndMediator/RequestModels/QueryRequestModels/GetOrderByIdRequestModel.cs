using System;
using CQRSAndMediator.ResponseModels.QueryResponseModels;
using MediatR;
using Newtonsoft.Json;

namespace CQRSAndMediator.RequestModels.QueryRequestModels
{
    public class GetOrderByIdRequestModel : IRequest<GetOrderByIdResponseModel>
    {
        [JsonProperty(PropertyName = "orderId")]
        public int OrderId { get; set; }
    }
}
