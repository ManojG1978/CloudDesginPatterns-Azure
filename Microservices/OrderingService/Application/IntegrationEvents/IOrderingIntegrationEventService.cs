using System.Threading.Tasks;
using BuildingBlocks.Events;

namespace OrderingService.Application.IntegrationEvents
{
    public interface IOrderingIntegrationEventService
    {
        void PublishThroughEventBus(IntegrationEvent evt);
    }
}
