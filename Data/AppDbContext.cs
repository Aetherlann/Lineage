using Microsoft.EntityFrameworkCore;
using MyDashboardApp.Models;

namespace MyDashboardApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}
        
        public DbSet<AnalysisConnection> AnalysisConnections { get; set; }
        public DbSet<ForeignKeyDependency> ForeignKeyDependencies { get; set; }
        public DbSet<SqlDependency> SqlDependencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure indexes for better query performance
            modelBuilder.Entity<ForeignKeyDependency>()
                .HasIndex(e => e.ParentTable);
            
            modelBuilder.Entity<ForeignKeyDependency>()
                .HasIndex(e => e.ChildTable);

            modelBuilder.Entity<SqlDependency>()
                .HasIndex(e => e.SourceObject);
            
            modelBuilder.Entity<SqlDependency>()
                .HasIndex(e => e.TargetObject);
        }
    }
}
