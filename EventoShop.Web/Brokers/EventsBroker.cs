using Microsoft.Azure.ServiceBus;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventoShop.Web.Brokers
{
    public class EventsBroker : IEventsBroker
    {
        private IQueueClient queueClient;

        public EventsBroker(IQueueClient queueClient)
        {
            this.queueClient = queueClient;
        }

        public async Task SendEventMessageAsync(Message message)
        {
            await this.queueClient.SendAsync(message);
        }

        public void ListenToEvents(Func<Message, CancellationToken, Task> eventHandler)
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                AutoComplete = false,
                MaxConcurrentCalls = 1
            };

            this.queueClient.RegisterMessageHandler(eventHandler, messageHandlerOptions);
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            return Task.CompletedTask;
        }
    }
}
