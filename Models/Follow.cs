using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniSocialMedia.Models
{
    [Table("Follows")]
    public class Follow
    {
        [Key]
        public int FollowID { get; set; }

        public int FollowerId { get; set; }
        public int FollowingId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}