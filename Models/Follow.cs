using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MiniSocialMedia.Models
{
    [Table("Follows")]
    public class Follow
    {
        [Key, Column(Order = 0)]
        public int FollowerId { get; set; }

        [Key, Column(Order = 1)]
        public int FollowingId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}