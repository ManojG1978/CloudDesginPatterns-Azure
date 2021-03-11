using System;
using OrderingService.Domain.Exceptions;
using OrderingService.Domain.SeedWork;

namespace OrderingService.Domain.AggregatesModel.BuyerAggregate
{
    public class PaymentMethod : Entity
    {
        private string _alias;
        private string _cardHolderName;
        private string _cardNumber;

        private int _cardTypeId;
        private DateTime _expiration;
        private string _securityNumber;


        protected PaymentMethod()
        {
        }

        public PaymentMethod(int cardTypeId, string alias, string cardNumber, string securityNumber,
            string cardHolderName, DateTime expiration)
        {
            _cardNumber = !string.IsNullOrWhiteSpace(cardNumber)
                ? cardNumber
                : throw new OrderingDomainException(nameof(cardNumber));
            _securityNumber = !string.IsNullOrWhiteSpace(securityNumber)
                ? securityNumber
                : throw new OrderingDomainException(nameof(securityNumber));
            _cardHolderName = !string.IsNullOrWhiteSpace(cardHolderName)
                ? cardHolderName
                : throw new OrderingDomainException(nameof(cardHolderName));

            if (expiration < DateTime.UtcNow) throw new OrderingDomainException(nameof(expiration));

            _alias = alias;
            _expiration = expiration;
            _cardTypeId = cardTypeId;
        }

        public CardType CardType { get; private set; }

        public bool IsEqualTo(int cardTypeId, string cardNumber, DateTime expiration)
        {
            return _cardTypeId == cardTypeId
                   && _cardNumber == cardNumber
                   && _expiration == expiration;
        }
    }
}