using System.Data.Entity;

namespace MiniSocialMedia.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext() : base("name=MyDbContext")
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
          
            base.OnModelCreating(modelBuilder);
        }
    }
}