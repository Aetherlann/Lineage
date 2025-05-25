namespace MyDashboardApp.Models
{
    public class LineageTreeData
    {
        public string SelectedObject { get; set; } = string.Empty;
        public LineageNode Root { get; set; } = new LineageNode();
        public List<LineageNode> UpstreamNodes { get; set; } = new List<LineageNode>();
        public int TotalUpstreamCount { get; set; } = 0;
        public List<LineageNode> DownstreamNodes { get; set; } = new List<LineageNode>();
        public int TotalDownstreamCount { get; set; } = 0;
        public List<LineageNode> Nodes { get; set; } = new List<LineageNode>();
        public List<Relationship> Relationships { get; set; } = new List<Relationship>();
    }
}
