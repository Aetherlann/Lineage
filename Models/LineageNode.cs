namespace MyDashboardApp.Models
{
    public class LineageNode
    {
        public string ObjectName { get; set; } = string.Empty;
        public string ObjectType { get; set; } = "TABLE"; // TABLE, VIEW, PROC, etc.
        public int Depth { get; set; }
        public string Path { get; set; } = string.Empty;
        public string LinkType { get; set; } = "FK"; // FK, SQL, VIEW, PROC
        public List<LineageNode> Children { get; set; } = new List<LineageNode>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        public int RowCount { get; set; }
        public DateTime? LastRefresh { get; set; }
        public bool IsOrphan { get; set; }
    }
}
