using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BuildingBlocks.Abstractions;

namespace BuildingBlocks
{
    public class EventBus : IEventBus
    {
        private readonly IList<Type> _subscribedEventHandlerTypes;
        private readonly IServiceProvider _serviceProvider;

        public EventBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _subscribedEventHandlerTypes = new List<Type>();
        }

        public IEventBus Subscribe<T>()
        {
            _subscribedEventHandlerTypes.Add(typeof(T));
            return this;
        }

        public async Task PublishAsync<T>(T @event)
        {
            foreach (var handler in GetHandlers<T>())
            {
                await handler.TryHandleAsync(@event).ConfigureAwait(false);
            }
        }

        private IEnumerable<ICanHandle<T>> GetHandlers<T>()
        {
            foreach (var handlerType in _subscribedEventHandlerTypes)
            {
                if (handlerType.IsAssignableTo(typeof(ICanHandle<T>)))
                {
                    yield return (ICanHandle<T>) _serviceProvider.GetService(handlerType);
                }
            }
        }

    }
}
