using System.ComponentModel.DataAnnotations;

namespace MyDashboardApp.Models
{
    public class AnalysisConnection
    {
        public int Id { get; set; }
        [Required] public string Source { get; set; } = null!;
        [Required] public string Destination { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
