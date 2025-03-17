namespace OMS_5D_Tech.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Feedbacks
    {
        public int id { get; set; }

        public int? user_id { get; set; }

        [Required]
        public string message { get; set; }

        public DateTime? created_at { get; set; }

        public virtual tbl_Users tbl_Users { get; set; }
    }
}
