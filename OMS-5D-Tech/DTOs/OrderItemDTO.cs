using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.DTOs
{
    public class OrderItemDTO
    {
        public int id { get; set; }

        public int? order_id { get; set; }

        public int? product_id { get; set; }

        public int quantity { get; set; }

        public decimal price { get; set; }
    }
}