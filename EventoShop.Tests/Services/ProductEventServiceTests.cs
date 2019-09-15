using EventoShop.Web.Brokers;
using EventoShop.Web.Models;
using EventoShop.Web.Services;
using Microsoft.Azure.ServiceBus;
using Moq;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tynamix.ObjectFiller;
using Xunit;

namespace EventoShop.Web.Tests.Services
{
    public class ProductEventServiceTests
    {
        private readonly Mock<IEventsBroker> eventsBroker;
        private readonly Mock<IStorageBroker> storageBroker;

        public ProductEventServiceTests()
        {
            this.eventsBroker = new Mock<IEventsBroker>();
            this.storageBroker = new Mock<IStorageBroker>();
        }

        [Fact]
        public async Task ShouldAddNewProduct()
        {
            // given
            Product product = new Filler<Product>().Create();

            var productExpirationEvent = new ProductExpirationEvent
            {
                ProductId = product.Id,
                ExpirationDate = product.ExpirationDate
            };

            string serializedProductExpirationEvent = 
                JsonConvert.SerializeObject(productExpirationEvent);

            var productExpirationEventMessage = new Message(Encoding.UTF8.GetBytes(serializedProductExpirationEvent))
            {
                ScheduledEnqueueTimeUtc = product.ExpirationDate.ToUniversalTime()
            };

            this.eventsBroker.Setup(broker => 
                broker.SendEventMessageAsync(It.Is<Message>(message =>
                    Encoding.UTF8.GetString(message.Body) == Encoding.UTF8.GetString(productExpirationEventMessage.Body)
                    && message.ScheduledEnqueueTimeUtc == productExpirationEventMessage.ScheduledEnqueueTimeUtc)));

            // when
            var productService = new ProductEventService(this.eventsBroker.Object, this.storageBroker.Object);
            await productService.CreateProductEventAsync(product);

            // then
            this.eventsBroker.Verify(broker =>
                broker.SendEventMessageAsync(It.Is<Message>(message =>
                    Encoding.UTF8.GetString(message.Body) == Encoding.UTF8.GetString(productExpirationEventMessage.Body)
                    && message.ScheduledEnqueueTimeUtc == productExpirationEventMessage.ScheduledEnqueueTimeUtc)), Times.Once);
        }

        [Fact]
        public void ShouldListenToScheduledEventsOnInitiation()
        {
            // when
            var productService = new ProductEventService(this.eventsBroker.Object, this.storageBroker.Object);

            // then
            this.eventsBroker.Verify(broker => broker.ListenToEvents(productService.ExpireProductAsync), Times.Once);
        }

        [Fact]
        public async Task ShouldUpdateProductAsExpired()
        {
            // given
            Product product = new Filler<Product>().Create();

            var productExpirationEvent = new ProductExpirationEvent
            {
                ProductId = product.Id,
                ExpirationDate = product.ExpirationDate
            };

            CancellationToken cancellationToken = new CancellationToken();

            Message message = new Message
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(productExpirationEvent)),
                ScheduledEnqueueTimeUtc = product.ExpirationDate.ToUniversalTime()
            };

            this.storageBroker.Setup(broker => broker.GetProductByIdAsync(product.Id))
                .ReturnsAsync(product);

            // when
            var productService = new ProductEventService(this.eventsBroker.Object, this.storageBroker.Object);
            await productService.ExpireProductAsync(message, cancellationToken);

            // then
            this.storageBroker.Verify(broker => broker.UpdateProductAsync(It.Is<Product>(
                storageProduct => storageProduct.Id == product.Id
                && storageProduct.Condition == ProductCondition.Expired
                && storageProduct.ExpirationDate == product.ExpirationDate)), Times.Once);

        }

        [Fact]
        public async Task ShouldNotUpdateProductAsExpiredIfExpirationDatesDidntMatch()
        {
            // given
            Product product = new Filler<Product>().Create();

            var productExpirationEvent = new ProductExpirationEvent
            {
                ProductId = product.Id,
                ExpirationDate = product.ExpirationDate.AddDays(1)
            };

            CancellationToken cancellationToken = new CancellationToken();

            Message message = new Message
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(productExpirationEvent)),
                ScheduledEnqueueTimeUtc = product.ExpirationDate.ToUniversalTime()
            };

            this.storageBroker.Setup(broker => broker.GetProductByIdAsync(product.Id))
                .ReturnsAsync(product);

            // when
            var productService = new ProductEventService(this.eventsBroker.Object, this.storageBroker.Object);
            await productService.ExpireProductAsync(message, cancellationToken);

            // then
            this.storageBroker.Verify(broker => broker.UpdateProductAsync(It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task ShouldNotUpdateProductAsExpiredIfProductWasRemoved()
        {
            // given
            Product noProduct = null;
            var productExpirationEvent = new Filler<ProductExpirationEvent>().Create();

            CancellationToken cancellationToken = new CancellationToken();

            Message message = new Message
            {
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(productExpirationEvent)),
                ScheduledEnqueueTimeUtc = DateTime.Now
            };

            this.storageBroker.Setup(broker => broker.GetProductByIdAsync(productExpirationEvent.ProductId))
                .ReturnsAsync(noProduct);

            // when
            var productService = new ProductEventService(this.eventsBroker.Object, this.storageBroker.Object);
            await productService.ExpireProductAsync(message, cancellationToken);

            // then
            this.storageBroker.Verify(broker => broker.UpdateProductAsync(It.IsAny<Product>()), Times.Never);

        }
    }
}
