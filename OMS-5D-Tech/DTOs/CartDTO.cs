using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.DTOs
{
    public class CartDTO
    {
        public int id { get; set; }

        public int? user_id { get; set; }

        public int? product_id { get; set; }

        public int quantity { get; set; }

        UserDTO UserDTO { get; set; }
        ProductDTO ProductDTO { get; set; }
    }
}