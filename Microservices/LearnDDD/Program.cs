using BasketService.Infrastructure.Repositories;
using BuildingBlocks;
using BuildingBlocks.Abstractions;
using OrderingService.Application.IntegrationEvents;
using OrderingService.Domain.AggregatesModel.BuyerAggregate;
using OrderingService.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BasketService.IntegrationEvents.EventHandling;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderingService.Application.IntegrationEvents.EventHandling;


namespace LearnDDD
{
    class Program
    {
        static Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();
            
            CreateOrder(host.Services);
            Console.ReadLine();
            
            return host.RunAsync();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((_, services) =>
                    services
                        .AddMediatR(typeof(UserCheckoutAcceptedIntegrationEventHandler).Assembly,
                            typeof(OrderStartedIntegrationEvent).Assembly)
                        .AddLogging(logging => logging.AddConsole())
                        .AddSingleton<IBasketRepository, FakeBasketRepository>()
                        .AddSingleton<IBuyerRepository, FakeBuyerRepository>()
                        .AddSingleton<IOrderRepository, FakeOrderRepository>()
                        .AddSingleton<ICanHandle<OrderStartedIntegrationEvent>, OrderStartedIntegrationEventHandler>()
                        .AddSingleton<ICanHandle<UserCheckoutAcceptedIntegrationEvent>,
                            UserCheckoutAcceptedIntegrationEventHandler>()
                        .AddSingleton<IOrderingIntegrationEventService, OrderingIntegrationEventService>()
                        .AddSingleton<IEventBus, EventBus>());

        private static void CreateOrder(IServiceProvider serviceProvider)
        {
            var eventBus = serviceProvider.GetService<IEventBus>();
            eventBus.Subscribe<OrderStartedIntegrationEventHandler>();
            eventBus.Subscribe<UserCheckoutAcceptedIntegrationEventHandler>();

            var basket = new CustomerBasket("1")
            {
                Items = new List<BasketItem>
                {
                    new BasketItem
                    {
                        Id = "1",
                        OldUnitPrice = 1.0,
                        PictureUrl = "1.jpg",
                        ProductId = "1",
                        ProductName = "Jacket",
                        Quantity = 1,
                        UnitPrice = 1
                    }
                },
                BuyerId = "1"
            };

            var eventMessage = new UserCheckoutAcceptedIntegrationEvent(Guid.NewGuid().ToString(), "test", "city",
                "street",
                "state", "country", "12345", "1111111111111111", "test",
                new DateTime(2018, 12, 12), "123", CardType.Amex.Id, "test", Guid.NewGuid(), basket);

            // Once basket is checkout, sends an integration event to
            // ordering.api to convert basket to order and proceeds with
            // order creation process
            eventBus.PublishAsync(eventMessage);
        }
    }
}