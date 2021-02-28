using System;
using Newtonsoft.Json;

namespace CQRSAndMediator.ResponseModels.QueryResponseModels
{
    public class GetOrderByIdResponseModel
    {
        [JsonProperty(PropertyName = "orderId")]
        public int OrderId { get; set; }
        
        [JsonProperty(PropertyName = "orderDate")]
        public DateTime OrderDate { get; set; }
        
        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }
        
        [JsonProperty(PropertyName = "amount")]
        public double Amount { get; set; }
        
        [JsonProperty(PropertyName = "productName")]
        public string ProductName { get; set; }

        [JsonProperty(PropertyName = "orderedBy")]
        public string OrderedBy { get; set; }

    }
}
