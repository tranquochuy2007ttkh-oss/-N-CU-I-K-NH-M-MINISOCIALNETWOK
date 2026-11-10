using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniSocialMedia.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [StringLength(50)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(100)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(100)]
        public string Email { get; set; } = "user@gmail.com";

        [StringLength(255)]
        public string Avatar { get; set; }

        [StringLength(255)]
        public string AvatarUrl { get; set; }

        [StringLength(510)]
        public string Bio { get; set; }

        // Sửa kiểu DateTime thành DateTime? (cho phép nhận NULL từ SQL)
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
    }
}