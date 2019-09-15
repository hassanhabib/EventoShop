using EventoShop.Web.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventoShop.Web.Brokers
{
    public interface IStorageBroker
    {
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task<List<Product>> GetProductsAsync();
        Task DeleteProductAsync(Product product);
        Task<Product> GetProductByIdAsync(Guid productId);
    }
}
