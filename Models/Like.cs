using MiniSocialMedia.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MiniSocialMedia.Models
{
    [Table("Likes")]
    public class Like
    {
        [Key, Column(Order = 0)]
        [ForeignKey("Post")]
        public int PostId { get; set; }

        [Key, Column(Order = 1)]
        [ForeignKey("User")]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual Post Post { get; set; }

        public virtual User User { get; set; }
    }
}