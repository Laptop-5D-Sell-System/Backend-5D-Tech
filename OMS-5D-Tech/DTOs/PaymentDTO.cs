using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.DTOs
{
    public class PaymentDTO
    {
        public int id { get; set; }

        public int? order_id { get; set; }

        [Required]
        [StringLength(50)]
        public string payment_method { get; set; }

        public DateTime? payment_date { get; set; }

        public float amount { get; set; }

        [StringLength(50)]
        public string status { get; set; }

        OrderDTO orderDTO { get; set; }
    }
}