using EventoShop.Web.Brokers;
using EventoShop.Web.Models;
using EventoShop.Web.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tynamix.ObjectFiller;
using Xunit;

namespace EventoShop.Web.Tests.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IStorageBroker> storageBrokerMock;
        private readonly Mock<IProductEventService> productEventMock;

        public ProductServiceTests()
        {
            this.storageBrokerMock = new Mock<IStorageBroker>();
            this.productEventMock = new Mock<IProductEventService>();
        }

        [Fact]
        public async Task ShouldAddNewProductAndCreateProductEvent()
        {
            // given
            Product product = new Filler<Product>().Create();

            this.storageBrokerMock.Setup(broker => broker.AddProductAsync(product));
            this.productEventMock.Setup(service => service.CreateProductEventAsync(product));

            // when
            var productService = new ProductService(this.storageBrokerMock.Object, this.productEventMock.Object);
            await productService.AddProductAsync(product);

            // then
            this.storageBrokerMock.Verify(broker => broker.AddProductAsync(product), Times.Once);
            this.productEventMock.Verify(service => service.CreateProductEventAsync(product), Times.Once);
        }

        [Fact]
        public async Task ShouldDeleteProduct()
        {
            // given
            Product product = new Filler<Product>().Create();

            this.storageBrokerMock.Setup(broker => broker.DeleteProductAsync(product));

            // when
            var productService = new ProductService(this.storageBrokerMock.Object, this.productEventMock.Object);
            await productService.DeleteProductAsync(product);

            // then
            this.storageBrokerMock.Verify(broker => broker.DeleteProductAsync(product), Times.Once);
        }

        [Fact]
        public async Task ShouldUpdateProduct()
        {
            // given
            Product product = new Filler<Product>().Create();

            this.storageBrokerMock.Setup(broker => broker.UpdateProductAsync(product));

            // when
            var productService = new ProductService(this.storageBrokerMock.Object, this.productEventMock.Object);
            await productService.UpdateProductAsync(product);

            // then
            this.storageBrokerMock.Verify(broker => broker.UpdateProductAsync(product), Times.Once);
        }

        [Fact]
        public async Task ShouldGetAllProducts()
        {
            // given
            int randomNumberOfProducts = new IntRange(min: 1, max: 10).GetValue();
            List<Product> expectedProducts = new Filler<Product>().Create(randomNumberOfProducts).ToList();

            this.storageBrokerMock.Setup(broker => broker.GetProductsAsync()).ReturnsAsync(expectedProducts);

            // when
            var productService = new ProductService(this.storageBrokerMock.Object, this.productEventMock.Object);
            List<Product> actualProducts = await productService.GetProductsAsync();

            // then
            this.storageBrokerMock.Verify(broker => broker.GetProductsAsync(), Times.Once);
            actualProducts.Should().BeEquivalentTo(expectedProducts);
        }

        [Fact]
        public async Task ShouldGetProductById()
        {
            // given
            Product expectedProduct = new Filler<Product>().Create();
            Guid productId = expectedProduct.Id;

            this.storageBrokerMock.Setup(broker => broker.GetProductByIdAsync(productId)).ReturnsAsync(expectedProduct);

            // when
            var productService = new ProductService(this.storageBrokerMock.Object, this.productEventMock.Object);
            Product actualProduct = await productService.GetProductByIdAsync(productId);

            // then
            this.storageBrokerMock.Verify(broker => broker.GetProductByIdAsync(productId), Times.Once);
            actualProduct.Should().BeEquivalentTo(expectedProduct);
        }
    }
}
