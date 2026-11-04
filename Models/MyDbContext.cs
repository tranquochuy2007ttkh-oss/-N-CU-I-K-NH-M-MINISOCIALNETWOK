using MiniSocialMedia.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MiniSocialMedia.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext() : base("MiniSocialContext")
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Like> Likes { get; set; }

        public DbSet<Follow> Follows { get; set; }
        public bool TestConnection()
        {
            return Database.Exists();
        }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Like>()
                .HasKey(l => new { l.PostId, l.UserId });

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Like>()
       .HasKey(l => new { l.PostId, l.UserId });

            // Khóa chính kép cho Follow
            modelBuilder.Entity<Follow>()
                .HasKey(f => new { f.FollowerId, f.FollowingId });

            base.OnModelCreating(modelBuilder);
        }
    }

}