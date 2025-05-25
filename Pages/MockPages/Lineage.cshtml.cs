using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyDashboardApp.Models;
using MyDashboardApp.Services;

namespace MyDashboardApp.Pages.MockPages
{
    public class LineageModel : PageModel
    {
        private readonly LineageService _lineageService;
        private readonly DataLoadingService _dataLoadingService;

        public LineageModel(LineageService lineageService, DataLoadingService dataLoadingService)
        {
            _lineageService = lineageService;
            _dataLoadingService = dataLoadingService;
        }

        [BindProperty(SupportsGet = true)]
        public string? SelectedObject { get; set; }

        public LineageTreeData? LineageData { get; set; }
        public List<string> AllObjects { get; set; } = new List<string>();
        public List<ForeignKeyDependency> RawFkDependencies { get; set; } = new List<ForeignKeyDependency>();
        public List<SqlDependency> RawSqlDependencies { get; set; } = new List<SqlDependency>();

        public async Task OnGetAsync()
        {
            try
            {
                AllObjects = await _lineageService.GetAllObjectsAsync();
                RawFkDependencies = await _lineageService.GetAllForeignKeyDependenciesAsync();
                RawSqlDependencies = await _lineageService.GetAllSqlDependenciesAsync();
                
                if (!string.IsNullOrEmpty(SelectedObject))
                {
                    LineageData = await _lineageService.GetLineageTreeAsync(SelectedObject);
                }
                else if (AllObjects.Any())
                {
                    // Default to first object if none selected
                    SelectedObject = AllObjects.First();
                    LineageData = await _lineageService.GetLineageTreeAsync(SelectedObject);
                }
            }
            catch (Exception ex)
            {
                // For debugging - in production you'd log this properly
                AllObjects = new List<string> { "Error: " + ex.Message };
                RawFkDependencies = new List<ForeignKeyDependency>();
                RawSqlDependencies = new List<SqlDependency>();
            }
        }

        public async Task<IActionResult> OnGetLineageDataAsync(string objectName)
        {
            try
            {
                var lineageData = await _lineageService.GetLineageTreeAsync(objectName);
                return new JsonResult(lineageData);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { error = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostLoadDataAsync()
        {
            try
            {
                await _dataLoadingService.LoadLineageDataAsync();
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                // For debugging - in production you'd log this properly
                AllObjects = new List<string> { "Error loading data: " + ex.Message };
                return Page();
            }
        }
    }
}
