using DataAccess.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Context
{
    
    public class PoetryDbContext : DbContext
    {
        public PoetryDbContext(DbContextOptions<PoetryDbContext> options) : base(options) { }

        public DbSet<Poem> Poems { get; set; } 
        public DbSet<PoemStatistics> PoemStatistics { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Collection> Collections { get; set; }
        public DbSet<SavedPoem> SavedPoems { get; set; }
        public DbSet<PoemNotification> PoemNotifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Poem configuration
            modelBuilder.Entity<Poem>()
                .HasOne(p => p.Statistics)
                .WithOne(s => s.Poem)
                .HasForeignKey<PoemStatistics>(s => s.PoemId);


            // Many-to-many relationships
            modelBuilder.Entity<Poem>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Poems);

            modelBuilder.Entity<Poem>()
                .HasMany(p => p.Categories)
                .WithMany(c => c.Poems);

            
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.Id = entry.Entity.Id == Guid.Empty ? Guid.NewGuid() : entry.Entity.Id;
                        break;
                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = now;
                        break;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}