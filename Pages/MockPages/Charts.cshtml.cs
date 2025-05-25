using Microsoft.AspNetCore.Mvc.RazorPages;
using MyDashboardApp.Data;
using MyDashboardApp.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MyDashboardApp.Pages.MockPages
{
    public class ChartsModel : PageModel
    {
        private readonly AppDbContext _context;

        public ChartsModel(AppDbContext context)
        {
            _context = context;
        }

        public List<AnalysisConnection> AnalysisConnections { get; set; }
        public Dictionary<string, int> SourceCounts { get; set; }

        public async Task OnGetAsync()
        {
            AnalysisConnections = await _context.AnalysisConnections.ToListAsync();

            SourceCounts = AnalysisConnections
                .GroupBy(a => a.Source)
                .ToDictionary(g => g.Key, g => g.Count());


        }
    }
}
