using CQRSAndMediator.ResponseModels.CommandResponseModels;
using MediatR;
using Newtonsoft.Json;

namespace CQRSAndMediator.RequestModels.CommandRequestModels
{
    public class MakeOrderRequestModel : IRequest<MakeOrderResponseModel>
    {
        [JsonProperty(PropertyName = "productName")]
        public string ProductName { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "orderedBy")]
        public string OrderedBy { get; set; }
    }
}