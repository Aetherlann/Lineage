using Microsoft.EntityFrameworkCore;
using MyDashboardApp.Data;
using MyDashboardApp.Models;
using MyDashboardApp.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=analysis.db")); // Ensure this matches your actual DB file
builder.Services.AddScoped<LineageService>();
builder.Services.AddScoped<DataLoadingService>(); // Assuming this service exists for data loading operations
var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate(); // Apply any pending migrations

        bool dataAdded = false;

        // Existing seeding for AnalysisConnections
        if (!context.AnalysisConnections.Any())
        {
            context.AnalysisConnections.AddRange(
                new AnalysisConnection { Source = "SourceA", Destination = "DestX", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceB", Destination = "DestY", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceA", Destination = "DestZ", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceC", Destination = "DestA", CreatedAt = DateTime.UtcNow },
                new AnalysisConnection { Source = "SourceB", Destination = "DestB", CreatedAt = DateTime.UtcNow }
            );
            dataAdded = true;
        }

        // Seeding for ForeignKeyDependencies
        if (!context.ForeignKeyDependencies.Any())
        {
            context.ForeignKeyDependencies.AddRange(
                new ForeignKeyDependency { ParentTable = "DimCustomer", ParentColumn = "CustomerID", ChildTable = "FactSales", ChildColumn = "CustomerID", CreatedAt = DateTime.UtcNow },
                new ForeignKeyDependency { ParentTable = "DimProduct", ParentColumn = "ProductID", ChildTable = "FactSales", ChildColumn = "ProductID", CreatedAt = DateTime.UtcNow },
                new ForeignKeyDependency { ParentTable = "DimDate", ParentColumn = "DateKey", ChildTable = "FactSales", ChildColumn = "OrderDateKey", CreatedAt = DateTime.UtcNow }
            );
            dataAdded = true;
        }

        // Seeding for SqlDependencies
        if (!context.SqlDependencies.Any())
        {
            context.SqlDependencies.AddRange(
                new SqlDependency { SourceObject = "RawOrders", TargetObject = "ViewRecentOrders", ObjectType = "VIEW", SqlTextSnippet = "SELECT OrderID, CustomerID, OrderDate, TotalAmount FROM RawOrders WHERE OrderDate > '2023-01-01'", CreatedAt = DateTime.UtcNow },
                new SqlDependency { SourceObject = "ViewRecentOrders", TargetObject = "ProcGenerateSalesReport", ObjectType = "PROC", SqlTextSnippet = "INSERT INTO SalesReport (ReportDate, TotalSales) SELECT GETDATE(), SUM(TotalAmount) FROM ViewRecentOrders", CreatedAt = DateTime.UtcNow },
                new SqlDependency { SourceObject = "FactSales", TargetObject = "ViewSalesSummary", ObjectType = "VIEW", SqlTextSnippet = "SELECT ProductID, SUM(SalesAmount) AS TotalSales FROM FactSales GROUP BY ProductID", CreatedAt = DateTime.UtcNow }
            );
            dataAdded = true;
        }

        if (dataAdded)
        {
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
