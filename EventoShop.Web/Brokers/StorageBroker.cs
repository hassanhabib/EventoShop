using EventoShop.Web.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventoShop.Web.Brokers
{
    public class StorageBroker : DbContext, IStorageBroker
    {
        public StorageBroker(DbContextOptions<StorageBroker> options) : base(options) { }

        public DbSet<Product> Products { get; set; }

        public async Task AddProductAsync(Product product)
        {
            await this.Products.AddAsync(product);
            await this.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(Product product)
        {
            this.Products.Remove(product);
            await this.SaveChangesAsync();
        }

        public async Task<Product> GetProductByIdAsync(Guid productId)
        {
            return await this.Products.FindAsync(productId);
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await this.Products.ToListAsync();
        }

        public async Task UpdateProductAsync(Product product)
        {
            this.Products.Update(product);
            await this.SaveChangesAsync();
        }


        public override void Dispose()
        {
            // TODO: handle disposing after background work is done
        }
    }
}
