using EventoShop.Web.Models;
using Microsoft.Azure.ServiceBus;
using System.Threading;
using System.Threading.Tasks;

namespace EventoShop.Web.Services
{
    public interface IProductEventService
    {
        Task CreateProductEventAsync(Product product);
        Task ExpireProductAsync(Message productMessage, CancellationToken cancellationToken);
    }
}
