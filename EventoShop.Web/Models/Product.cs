using System;

namespace EventoShop.Web.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ProductCondition Condition { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
