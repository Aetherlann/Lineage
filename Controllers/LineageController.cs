using Microsoft.AspNetCore.Mvc;
using MyDashboardApp.Services;
using MyDashboardApp.Models;

namespace MyDashboardApp.Controllers
{
    public class LineageController : Controller
    {
        private readonly ILineageService _lineageService;

        public LineageController(ILineageService lineageService)
        {
            _lineageService = lineageService;
        }

        public IActionResult Index(string objectName)
        {
            var lineageData = _lineageService.GetLineageTreeAsync(objectName).Result;
            return View(lineageData);
        }
    }
}
