using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public int? CustomerID { get; set; }

        public DateTime Date { get; set; }
        public string ShippingType { get; set; }
        public decimal ShippingPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal VAT { get; set; }

        // Navigation
        public Customer? Customer { get; set; }
        public List<OrderRow> OrderRows { get; set; } = new();
    }


}
