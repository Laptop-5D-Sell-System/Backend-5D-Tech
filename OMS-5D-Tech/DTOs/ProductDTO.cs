using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.DTOs
{
    public class ProductDTO
    {
        public int id { get; set; }

        [Required]
        [StringLength(255)]
        public string name { get; set; }

        public string description { get; set; }

        public decimal price { get; set; }

        [Required]
        [StringLength(255)]
        public string product_image { get; set; }

        public int stock_quantity { get; set; }

        public int? category_id { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }
        CategoryDTO category { get; set; }
    }
}