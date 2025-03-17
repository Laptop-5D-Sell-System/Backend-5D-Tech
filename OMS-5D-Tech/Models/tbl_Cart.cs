namespace OMS_5D_Tech.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Cart
    {
        public int id { get; set; }

        public int? user_id { get; set; }

        public int? product_id { get; set; }

        public int quantity { get; set; }

        public virtual tbl_Products tbl_Products { get; set; }

        public virtual tbl_Users tbl_Users { get; set; }
    }
}
