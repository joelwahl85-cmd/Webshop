using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Models
{
    public class OrderRow
    {
        public int OrderRowID { get; set; }

        public int OrderID { get; set; }
        public int ProductID { get; set; }

        public int Quantity { get; set; }
        public decimal PriceEach { get; set; }

        // Navigation
        public Order Order { get; set; }
        public Product Product { get; set; }
    }


}
