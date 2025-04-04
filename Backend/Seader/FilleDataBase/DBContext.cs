using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilleDataBase
{
    public class PoetryDbContext : DbContext
    {
        public DbSet<Poem> Poems { get; set; }

        // Default constructor
        public PoetryDbContext()
        {
        }

        // Constructor that accepts options
        public PoetryDbContext(DbContextOptions<PoetryDbContext> options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // Only configure if options haven't been set elsewhere
            if (!options.IsConfigured)
            {
                // MySQL connection string - using only Pomelo provider
                string connectionString = "server=localhost;port=3306;database=content;user=root;password=0000;";
                options.UseMySql(connectionString, MySqlServerVersion.LatestSupportedServerVersion);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the Poem entity to use LinesJson for serializing the Lines collection
            modelBuilder.Entity<Poem>()
                .Property(p => p.LinesJson)
                .HasColumnName("Lines"); // This makes the column name in the DB still be "Lines"

            // Configure new properties with reasonable defaults
            modelBuilder.Entity<Poem>()
                .Property(p => p.Description)
                .IsRequired(false); // Make description nullable

            modelBuilder.Entity<Poem>()
                .Property(p => p.PublicationDate)
                .IsRequired(false); // Make publication date nullable

            base.OnModelCreating(modelBuilder);
        }
    }
}