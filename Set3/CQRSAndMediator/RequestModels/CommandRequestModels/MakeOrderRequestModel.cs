using System;
using CQRSAndMediator.ResponseModels.CommandResponseModels;
using MediatR;
using Newtonsoft.Json;

namespace CQRSAndMediator.RequestModels.CommandRequestModels
{
    public class MakeOrderRequestModel : IRequest<MakeOrderResponseModel>
    {
        [JsonProperty(PropertyName = "orderId")]
        public int OrderId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        
        [JsonProperty(PropertyName = "orderDate")]
        public DateTime OrderDate { get; set; }

        [JsonProperty(PropertyName = "productId")]
        public Guid ProductId { get; set; }

        [JsonProperty(PropertyName = "productName")]
        public string ProductName { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public double Amount { get; set; }

        [JsonProperty(PropertyName = "orderedById")]
        public Guid OrderedById { get; set; }
        
        [JsonProperty(PropertyName = "orderedBy")]
        public string OrderedBy { get; set; }

    }
}