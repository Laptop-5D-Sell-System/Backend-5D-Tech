namespace OMS_5D_Tech.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Payments
    {
        public int id { get; set; }

        public int? order_id { get; set; }

        [Required]
        [StringLength(50)]
        public string payment_method { get; set; }

        public DateTime? payment_date { get; set; }

        public decimal amount { get; set; }

        [StringLength(50)]
        public string status { get; set; }

        public virtual tbl_Orders tbl_Orders { get; set; }
    }
}
