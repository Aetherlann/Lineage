using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyDashboardApp.Data;
using MyDashboardApp.Models;

namespace MyDashboardApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;
        public IndexModel(AppDbContext db) => _db = db;

        [BindProperty]
        public AnalysisConnection NewConn { get; set; } = new();

        public List<AnalysisConnection> AllConns { get; set; } = new();
        public int TotalTablesAnalysed { get; set; }
        public int ClientPIITables { get; set; }

        public void OnGet()
        {
            AllConns = _db.AnalysisConnections.OrderByDescending(c => c.CreatedAt).ToList();
            TotalTablesAnalysed = 125; // Mock data
            ClientPIITables = 12; // Mock data
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid) { OnGet(); return Page(); }
            NewConn.CreatedAt = DateTime.UtcNow;
            _db.AnalysisConnections.Add(NewConn);
            _db.SaveChanges();
            return RedirectToPage();
        }
    }
}
