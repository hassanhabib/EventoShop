using EventoShop.Web.Brokers;
using EventoShop.Web.Models;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EventoShop.Web.Services
{
    public class ProductEventService : IProductEventService
    {
        private readonly IEventsBroker eventsBoker;
        private readonly IStorageBroker storageBroker;

        public ProductEventService(IEventsBroker eventsBoker, IStorageBroker storageBroker)
        {
            this.eventsBoker = eventsBoker;
            this.storageBroker = storageBroker;

            this.ListenToProductEvents();
        }

        public async Task CreateProductEventAsync(Product product)
        {
            Message productMessage = CreateProductAsMessage(product);
            await this.eventsBoker.SendEventMessageAsync(productMessage);
        }

        public void ListenToProductEvents()
        {
            this.eventsBoker.ListenToEvents(ExpireProductAsync);
        }

        private Message CreateProductAsMessage(Product product)
        {
            var productExpirationEvent = new ProductExpirationEvent
            {
                ProductId = product.Id,
                ExpirationDate = product.ExpirationDate
            };

            string serializedProductExpirationEvent = JsonConvert.SerializeObject(productExpirationEvent);
            byte[] productBinary = Encoding.UTF8.GetBytes(serializedProductExpirationEvent);

            return new Message(productBinary)
            {
                ScheduledEnqueueTimeUtc = product.ExpirationDate.ToUniversalTime()
            };
        }

        public async Task ExpireProductAsync(Message productMessage, CancellationToken cancellationToken)
        {
            string serializedProductExpirationEvent = Encoding.UTF8.GetString(productMessage.Body);
            ProductExpirationEvent productExpirationEvent = 
                JsonConvert.DeserializeObject<ProductExpirationEvent>(serializedProductExpirationEvent);


            Product product = await this.storageBroker.GetProductByIdAsync(productExpirationEvent.ProductId);
            if (product != null && product.ExpirationDate == productExpirationEvent.ExpirationDate)
            {
                product.Condition = ProductCondition.Expired;
                await this.storageBroker.UpdateProductAsync(product);
            }
        }
    }
}
