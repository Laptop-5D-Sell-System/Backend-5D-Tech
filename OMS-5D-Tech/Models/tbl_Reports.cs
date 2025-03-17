namespace OMS_5D_Tech.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class tbl_Reports
    {
        public int id { get; set; }

        [Required]
        [StringLength(50)]
        public string report_type { get; set; }

        [Required]
        public string content { get; set; }

        public DateTime? generated_at { get; set; }
    }
}
