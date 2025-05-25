using Microsoft.EntityFrameworkCore;
using MyDashboardApp.Models;

namespace MyDashboardApp.Data
{
    public class NewDbLineageContext : DbContext
    {
        public NewDbLineageContext(DbContextOptions<NewDbLineageContext> options) : base(options) {}

        public DbSet<ForeignKeyDependency> ForeignKeyDependencies { get; set; }
        public DbSet<SqlDependency> SqlDependencies { get; set; }

        // If AnalysisConnections table also exists in New.sqlite and is needed, it could be added:
        // public DbSet<AnalysisConnection> AnalysisConnections { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Add any specific model configurations for New.sqlite if necessary.
            // For example, if table names in New.sqlite are different from DbSet property names,
            // or if specific schemas or keys need to be defined.
            // If New.sqlite's schema is pre-defined and matches the models, this can be minimal.

            // Example index configuration (if applicable and managed by EF Core for New.sqlite):
            /*
            modelBuilder.Entity<ForeignKeyDependency>()
                .HasIndex(e => e.ParentTable);
            modelBuilder.Entity<ForeignKeyDependency>()
                .HasIndex(e => e.ChildTable);

            modelBuilder.Entity<SqlDependency>()
                .HasIndex(e => e.SourceObject);
            modelBuilder.Entity<SqlDependency>()
                .HasIndex(e => e.TargetObject);
            */
        }
    }
}
