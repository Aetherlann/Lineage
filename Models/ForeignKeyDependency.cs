using System.ComponentModel.DataAnnotations;

namespace MyDashboardApp.Models
{
    public class ForeignKeyDependency
    {
        public int Id { get; set; }
        
        [Required]
        public string ParentTable { get; set; } = null!;
        
        [Required]
        public string ChildTable { get; set; } = null!;
        
        [Required]
        public string ParentColumn { get; set; } = null!;
        
        [Required]
        public string ChildColumn { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
