# Razor Pages Dashboard with Custom CSS

- ASP.NET Core Razor Pages + EF Core SQLite
- Dashboard styled with Tailwind + custom site.css
- No inline hardcoded colors—moved to CSS

## Setup
```bash
dotnet tool install --global dotnet-ef
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```
Open http://localhost:5000 or https://localhost:5001.
