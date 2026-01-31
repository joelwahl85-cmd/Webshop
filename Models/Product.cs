using System.Collections.Generic;

namespace Webshop.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        public int CategoryID { get; set; }
        public Category Category { get; set; }
        public bool IsFeatured { get; set; }
        public int Stock { get; set; }

        // Navigation
        public ICollection<OrderRow> OrderRows { get; set; }
    }
}
