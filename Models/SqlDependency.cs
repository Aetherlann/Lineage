using System.ComponentModel.DataAnnotations;

namespace MyDashboardApp.Models
{
    public class SqlDependency
    {
        public int Id { get; set; }
        
        [Required]
        public string SourceObject { get; set; } = null!;
        
        [Required]
        public string TargetObject { get; set; } = null!;
        
        public string SqlTextSnippet { get; set; } = string.Empty;
        
        public string ObjectType { get; set; } = "TABLE"; // TABLE, VIEW, PROC, etc.
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
