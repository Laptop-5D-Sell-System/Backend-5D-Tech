using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OMS_5D_Tech.DTOs
{
    public class AccountDTO
    {
        [Required(ErrorMessage = "Email không được để trống!")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ!")]
        [StringLength(255)]
        public string email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự!")]
        [StringLength(255)]
        public string password_hash { get; set; }

        [StringLength(255)]
        public string refresh_token { get; set; }

        public int id { get; set; }

        public DateTime? created_at { get; set; }

        public DateTime? updated_at { get; set; }

        public bool? is_active { get; set; } = false;

        public bool? is_verified { get; set; } = false;

        [StringLength(50)]
        public string role { get; set; } = "user";

        public DateTime? refresh_token_expiry { get; set; }

        UserDTO User { get; set; } 
    }
}