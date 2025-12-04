using System.Threading.Tasks;
using Agora.Domain.Events;

namespace Agora.Domain.Interfaces
{
    public interface IEventBus
    {
        Task Publish(IntegrationEvent @event);
        Task Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }

    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler
    {
    }
}
