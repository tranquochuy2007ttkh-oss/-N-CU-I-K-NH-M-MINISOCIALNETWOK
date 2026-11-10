using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniSocialMedia.Models
{
    [Table("Comments")]
    public class Comment
    {
        [Key]
        public int CommentID { get; set; }

        public int PostID { get; set; }
        public int UserID { get; set; }

        [Required(ErrorMessage = "Nội dung bình luận không được để trống")]
        [StringLength(1000, ErrorMessage = "Bình luận tối đa 1000 ký tự")]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("PostID")]
        public virtual Post Post { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}