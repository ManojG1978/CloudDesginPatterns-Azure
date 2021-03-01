using System;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace OrdersMaterializedViewCreator
{
    public class OrderModel
    {
        [JsonProperty(PropertyName = "orderId")]
        public int OrderId { get; set; }

        [JsonProperty(PropertyName = "id")]
        public Guid Id { get; set; }
        
        [JsonProperty(PropertyName = "orderDate")]
        public DateTime OrderDate { get; set; }

        [JsonProperty(PropertyName = "productName")]
        public string ProductName { get; set; }

        [JsonProperty(PropertyName = "quantity")]
        public int Quantity { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public double Amount { get; set; }

        [JsonProperty(PropertyName = "orderedBy")]
        public string OrderedBy { get; set; }
        
        public static OrderModel FromDocument(Document document)
        {
            var result = new OrderModel()
            {
                OrderId = document.GetPropertyValue<int>("orderId"),
                Amount = document.GetPropertyValue<double>("amount"),
                OrderDate = document.GetPropertyValue<DateTime>("orderDate"),
                ProductName = document.GetPropertyValue<string>("productName"),
                Quantity = document.GetPropertyValue<int>("quantity"),
                Id = document.GetPropertyValue<Guid>("id"),
                OrderedBy = document.GetPropertyValue<string>("orderedBy")
            };

            return result;
        }

        public static Document ToDocument(OrderModel order, Document document)
        {
            document ??= new Document();
            document.SetPropertyValue("id", order.Id.ToString());
            document.SetPropertyValue("orderId", order.OrderId);
            document.SetPropertyValue("amount", order.Amount);
            document.SetPropertyValue("orderedBy", order.OrderedBy);
            document.SetPropertyValue("productName", order.ProductName);
            document.SetPropertyValue("orderDate", order.OrderDate);
            document.SetPropertyValue("quantity", order.Quantity);

            return document;

        }
    }
    
        
}