using Microsoft.EntityFrameworkCore;
using MyDashboardApp.Data;
using MyDashboardApp.Models;
using MyDashboardApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=analysis.db"));
builder.Services.AddScoped<LineageService>();
builder.Services.AddScoped<DataLoadingService>();
var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate(); // Apply any pending migrations
        if (!context.AnalysisConnections.Any())
        {
            context.AnalysisConnections.AddRange(
                new AnalysisConnection { Source = "SourceA", Destination = "DestX", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceB", Destination = "DestY", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceA", Destination = "DestZ", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceC", Destination = "DestA", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceB", Destination = "DestB", CreatedAt = DateTime.UtcNow }
            );
            context.SaveChanges();
        }

    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}


if (!app.Environment.IsProduction()) app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();
