using Microsoft.Azure.ServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventoShop.Web.Brokers
{
    public interface IEventsBroker
    {
        Task SendEventMessageAsync(Message message);
        void ListenToEvents(Func<Message, CancellationToken, Task> eventHandler);
    }
}
