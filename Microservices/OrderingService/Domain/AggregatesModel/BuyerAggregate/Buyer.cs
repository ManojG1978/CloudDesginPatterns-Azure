using System;
using System.Collections.Generic;
using System.Linq;
using OrderingService.Domain.Events;
using OrderingService.Domain.SeedWork;

namespace OrderingService.Domain.AggregatesModel.BuyerAggregate
{
    public class Buyer : Entity, IAggregateRoot
    {
        private readonly List<PaymentMethod> _paymentMethods;

        protected Buyer()
        {
            _paymentMethods = new List<PaymentMethod>();
        }

        public Buyer(string identity, string name) : this()
        {
            Identity = !string.IsNullOrWhiteSpace(identity)
                ? identity
                : throw new ArgumentNullException(nameof(identity));
            Name = !string.IsNullOrWhiteSpace(name) ? name : throw new ArgumentNullException(nameof(name));
        }

        public string Identity { get; private set; }

        public string Name { get; private set; }

        public IEnumerable<PaymentMethod> PaymentMethods => _paymentMethods.AsReadOnly();

        public PaymentMethod VerifyOrAddPaymentMethod(
            int cardTypeId, string alias, string cardNumber,
            string securityNumber, string cardHolderName, DateTime expiration, int orderId)
        {
            var existingPayment = _paymentMethods
                .SingleOrDefault(p => p.IsEqualTo(cardTypeId, cardNumber, expiration));

            if (existingPayment != null)
            {
                AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, existingPayment, orderId));

                return existingPayment;
            }

            var payment = new PaymentMethod(cardTypeId, alias, cardNumber, securityNumber, cardHolderName, expiration);

            _paymentMethods.Add(payment);

            AddDomainEvent(new BuyerAndPaymentMethodVerifiedDomainEvent(this, payment, orderId));

            return payment;
        }
    }
}