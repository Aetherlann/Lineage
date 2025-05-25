using MyDashboardApp.Services;
using MyDashboardApp.Models;

namespace MyDashboardApp.Services
{
    public interface ILineageService
    {
        Task<LineageTreeData> GetLineageTreeAsync(string objectName);
    }
}
