using System.Threading.Tasks;

namespace BuildingBlocks.Abstractions
{
    public interface ICanHandle<in T>
    {
        Task<bool> TryHandleAsync(T @event);
    }
}
