using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.DTOs
{
    public class UserDTO
    {
        public int id { get; set; }

        public int? account_id { get; set; }

        [StringLength(100)]
        public string first_name { get; set; }

        [StringLength(100)]
        public string last_name { get; set; }

        [Column(TypeName = "date")]
        public DateTime? dob { get; set; }

        [StringLength(20)]
        public string phone_number { get; set; }

        public string address { get; set; }

        [StringLength(255)]
        public string profile_picture { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }
    }

}