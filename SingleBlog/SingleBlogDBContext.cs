using Microsoft.EntityFrameworkCore;
using SingleBlog.Entities;

namespace SingleBlog
{
    public class SingleBlogDBContext : DbContext
    {
        public SingleBlogDBContext(DbContextOptions options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<PostEntity> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostEntity>()
                .HasMany(dm => dm.TagEntities)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
