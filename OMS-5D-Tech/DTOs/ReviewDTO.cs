using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.DTOs
{
    public class ReviewDTO
    {
        public int id { get; set; }

        public int? user_id { get; set; }

        public int? product_id { get; set; }

        public int? rating { get; set; }

        [StringLength(255)]
        public string comment { get; set; }

        public DateTime? created_at { get; set; }
    }
}