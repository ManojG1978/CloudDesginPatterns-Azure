using System.Threading.Tasks;

namespace BuildingBlocks.Abstractions
{
    public interface IEventBus
    {
        IEventBus Subscribe<T>();
        Task PublishAsync<T>(T @event);
    }
}