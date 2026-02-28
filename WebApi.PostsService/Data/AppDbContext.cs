using Microsoft.EntityFrameworkCore;
using WebApi.PostsService.Models;

namespace WebApi.PostsService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Post> Posts => Set<Post>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Reaction> Reactions => Set<Reaction>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Tag> Tags => Set<Tag>();
        public DbSet<PostCategory> PostCategories => Set<PostCategory>();
        public DbSet<PostTag> PostTags => Set<PostTag>();

        protected override void OnModelCreating(ModelBuilder builder)
        {

            builder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });

            builder.Entity<PostCategory>()
                .HasKey(pc => new { pc.PostId, pc.CategoryId });

            builder.Entity<Reaction>()
                .HasKey(r => new { r.PostId, r.UserId });

        }

    }
}
