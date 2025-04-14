using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.DTOs
{
    public class OrderDTO
    {
        public int id { get; set; }

        public int? user_id { get; set; }
        public int? quantity { get; set; }

        public DateTime? order_date { get; set; }

        [StringLength(50)]
        public string status { get; set; }

        public decimal total { get; set; }
        public List<OrderItemDTO> OrderItems { get; set; }
    }
}