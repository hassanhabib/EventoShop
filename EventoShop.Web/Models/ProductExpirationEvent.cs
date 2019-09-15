using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventoShop.Web.Models
{
    public class ProductExpirationEvent
    {
        public Guid ProductId { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
