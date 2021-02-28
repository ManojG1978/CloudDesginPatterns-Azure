using System;
using Newtonsoft.Json;

namespace CQRSAndMediator.ResponseModels.CommandResponseModels
{
    public class MakeOrderResponseModel
    {
        [JsonProperty(PropertyName = "isSuccess")]
        public bool IsSuccess { get; set; }
        
        [JsonProperty(PropertyName = "orderId")]
        public int OrderId { get; set; }
    }
}
