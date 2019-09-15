using EventoShop.Web.Brokers;
using EventoShop.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventoShop.Web.Services
{
    public class ProductService : IProductService
    {
        private readonly IStorageBroker storageBroker;
        private readonly IProductEventService productEventService;

        public ProductService(IStorageBroker storageBroker, IProductEventService productEventService)
        {
            this.storageBroker = storageBroker;
            this.productEventService = productEventService;
        }

        public async Task AddProductAsync(Product product)
        {
            await this.storageBroker.AddProductAsync(product);
            await this.productEventService.CreateProductEventAsync(product);
        }

        public async Task DeleteProductAsync(Product product)
        {
            await this.storageBroker.DeleteProductAsync(product);
        }

        public async Task<Product> GetProductByIdAsync(Guid productId)
        {
            return await this.storageBroker.GetProductByIdAsync(productId);
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await this.storageBroker.GetProductsAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            await this.storageBroker.UpdateProductAsync(product);
        }
    }
}
