using Microsoft.EntityFrameworkCore;
using MyDashboardApp.Data;
using MyDashboardApp.Models;
using MyDashboardApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

// Existing AppDbContext registration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=analysis.db"));

// New NewDbLineageContext registration
builder.Services.AddDbContext<NewDbLineageContext>(options =>
    options.UseSqlite("Data Source=New.sqlite"));

builder.Services.AddScoped<LineageService>();
builder.Services.AddScoped<DataLoadingService>(); // Assuming this service exists for data loading operations
var app = builder.Build();

// Seed data for AppDbContext (analysis.db)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate(); // Apply any pending migrations for AppDbContext

        // Seeding for AnalysisConnections (kept as per instructions, assuming it might serve other purposes)
        if (!context.AnalysisConnections.Any())
        {
            context.AnalysisConnections.AddRange(
                new AnalysisConnection { Source = "SourceA", Destination = "DestX", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceB", Destination = "DestY", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceA", Destination = "DestZ", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceC", Destination = "DestA", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceB", Destination = "DestB", CreatedAt = DateTime.UtcNow }
            );
            context.SaveChanges(); // Save changes specifically for AnalysisConnections if they were added
        }

        // ForeignKeyDependencies and SqlDependencies seeding REMOVED from AppDbContext seeding logic.
        // These are now assumed to come from New.sqlite via NewDbLineageContext.

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the AppDbContext (analysis.db).");
    }
}


if (!app.Environment.IsProduction()) app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
