using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniSocialMedia.Models
{
    [Table("Likes")]
    public class Like
    {
        [Key]
        public int LikeID { get; set; }

        public int PostID { get; set; }
        public int UserID { get; set; }

        // Lưu loại cảm xúc: "Like", "Love", "Care", "Haha", "Wow", "Sad", "Angry"
        [StringLength(20)]
        public string ReactionType { get; set; } = "Like";

        public DateTime? CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("PostID")]
        public virtual Post Post { get; set; }

        [ForeignKey("UserID")]
        public virtual User User { get; set; }
    }
}