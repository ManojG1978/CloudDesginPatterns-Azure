using System;
using System.Collections.Generic;
using System.Linq;
using OrderingService.Domain.Events;
using OrderingService.Domain.SeedWork;

namespace OrderingService.Domain.AggregatesModel.OrderAggregate
{
    public class Order : Entity, IAggregateRoot
    {
        // DDD Patterns comment
        // Using a private collection field, better for DDD Aggregate's encapsulation
        // so OrderItems cannot be added from "outside the AggregateRoot" directly to the collection,
        // but only through the method OrderAggregateRoot.AddOrderItem() which includes behaviour.
        private List<OrderItem> _orderItems;

        private string _description;

        // DDD Patterns comment
        // Using private fields, allowed since EF Core 1.1, is a much better encapsulation
        // aligned with DDD Aggregates and Domain Entities (Instead of properties and property collections)
        private DateTime _orderDate;
        private int _orderStatusId;
        private int _paymentId;

        // Address is a Value Object pattern example persisted as EF Core 2.0 owned entity
        public Address Address { get;  set; }

        public int? BuyerId { get;  set; }

        public OrderStatus OrderStatus
        {
            get; 
            set;
        }
        public IReadOnlyCollection<OrderItem> OrderItems => _orderItems;

        protected Order()
        {
            _orderItems = new List<OrderItem>();
        }

        public Order(string userId, string userName, Address address, int cardTypeId, string cardNumber,
            string cardSecurityNumber,
            string cardHolderName, DateTime cardExpiration, int? buyerId = null, int? paymentMethodId = null) : this()
        {
            BuyerId = buyerId;
            OrderStatus = OrderStatus.Submitted;
            _orderStatusId = OrderStatus.Submitted.Id;
            _orderDate = DateTime.UtcNow;
            Address = address;

            // Add the OrderStarterDomainEvent to the domain events collection 
            // to be raised/dispatched when committing changes into the Database [ After DbContext.SaveChanges() ]
            AddOrderStartedDomainEvent(userId, userName, cardTypeId, cardNumber,
                cardSecurityNumber, cardHolderName, cardExpiration);
        }

        // DDD Patterns comment
        // This Order AggregateRoot's method "AddOrderItem()" should be the only way to add Items to the Order,
        // so any behavior (discounts, etc.) and validations are controlled by the AggregateRoot 
        // in order to maintain consistency between the whole Aggregate. 
        public void AddOrderItem(int productId, string productName, decimal unitPrice, decimal discount,
            string pictureUrl, int units = 1)
        {
            var existingOrderForProduct = _orderItems.SingleOrDefault(o => o.ProductId == productId);

            if (existingOrderForProduct != null)
            {
                //if previous line exist modify it with higher discount  and units..

                if (discount > existingOrderForProduct.GetCurrentDiscount())
                    existingOrderForProduct.SetNewDiscount(discount);

                existingOrderForProduct.AddUnits(units);
            }
            else
            {
                //add validated new order item

                var orderItem = new OrderItem(productId, productName, unitPrice, discount, pictureUrl, units);
                _orderItems.Add(orderItem);
            }
        }

        public void SetBuyerId(int id)
        {
            BuyerId = id;
        }

        public void SetPaymentId(int paymentId)
        {
            _paymentId = paymentId;
        }

        private void AddOrderStartedDomainEvent(string userId, string userName, int cardTypeId, string cardNumber,
            string cardSecurityNumber, string cardHolderName, DateTime cardExpiration)
        {
            var orderStartedDomainEvent = new OrderStartedDomainEvent(this, userId, userName, cardTypeId,
                cardNumber, cardSecurityNumber,
                cardHolderName, cardExpiration);

            AddDomainEvent(orderStartedDomainEvent);
        }
    }
}