using Microsoft.EntityFrameworkCore;
using MyDashboardApp.Data;
using MyDashboardApp.Models;
using Microsoft.Data.Sqlite;

namespace MyDashboardApp.Services
{
    public class LineageService
    {
        private readonly AppDbContext _context;

        public LineageService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LineageTreeData> GetLineageTreeAsync(string objectName, int maxDepth = 5)
        {
            var treeData = new LineageTreeData
            {
                SelectedObject = objectName,
                Root = new LineageNode
                {
                    ObjectName = objectName,
                    ObjectType = await GetObjectTypeAsync(objectName),
                    Depth = 0,
                    Path = objectName
                }
            };

            // Get upstream lineage
            treeData.UpstreamNodes = await GetUpstreamLineageAsync(objectName, maxDepth);
            treeData.TotalUpstreamCount = treeData.UpstreamNodes.Count;

            // Get downstream lineage
            treeData.DownstreamNodes = await GetDownstreamLineageAsync(objectName, maxDepth);
            treeData.TotalDownstreamCount = treeData.DownstreamNodes.Count;

            return treeData;
        }

        private async Task<List<LineageNode>> GetUpstreamLineageAsync(string objectName, int maxDepth)
        {
            var visited = new HashSet<string>();
            var result = new List<LineageNode>();

            await TraverseUpstreamAsync(objectName, 0, maxDepth, visited, result, objectName);
            return result;
        }

        private async Task TraverseUpstreamAsync(string currentObject, int currentDepth, int maxDepth, 
            HashSet<string> visited, List<LineageNode> result, string rootPath)
        {
            if (currentDepth >= maxDepth || visited.Contains(currentObject))
                return;

            visited.Add(currentObject);

            // Get FK dependencies (this object as child)
            var fkDeps = await _context.ForeignKeyDependencies
                .Where(fk => fk.ChildTable == currentObject)
                .ToListAsync();

            foreach (var fk in fkDeps)
            {
                var node = new LineageNode
                {
                    ObjectName = fk.ParentTable,
                    ObjectType = await GetObjectTypeAsync(fk.ParentTable),
                    Depth = currentDepth + 1,
                    Path = $"{rootPath} ← {fk.ParentTable}",
                    LinkType = "FK",
                    Metadata = new Dictionary<string, object>
                    {
                        ["ParentColumn"] = fk.ParentColumn,
                        ["ChildColumn"] = fk.ChildColumn
                    }
                };

                result.Add(node);
                await TraverseUpstreamAsync(fk.ParentTable, currentDepth + 1, maxDepth, visited, result, rootPath);
            }

            // Get SQL dependencies (this object as target)
            var sqlDeps = await _context.SqlDependencies
                .Where(sql => sql.TargetObject == currentObject)
                .ToListAsync();

            foreach (var sql in sqlDeps)
            {
                var node = new LineageNode
                {
                    ObjectName = sql.SourceObject,
                    ObjectType = sql.ObjectType,
                    Depth = currentDepth + 1,
                    Path = $"{rootPath} ← {sql.SourceObject}",
                    LinkType = "SQL",
                };

                result.Add(node);
                await TraverseUpstreamAsync(sql.SourceObject, currentDepth + 1, maxDepth, visited, result, rootPath);
            }
        }

        private async Task<List<LineageNode>> GetDownstreamLineageAsync(string objectName, int maxDepth)
        {
            var visited = new HashSet<string>();
            var result = new List<LineageNode>();

            await TraverseDownstreamAsync(objectName, 0, maxDepth, visited, result, objectName);
            return result;
        }

        private async Task TraverseDownstreamAsync(string currentObject, int currentDepth, int maxDepth,
            HashSet<string> visited, List<LineageNode> result, string rootPath)
        {
            if (currentDepth >= maxDepth || visited.Contains(currentObject))
                return;

            visited.Add(currentObject);

            // Get FK dependencies (this object as parent)
            var fkDeps = await _context.ForeignKeyDependencies
                .Where(fk => fk.ParentTable == currentObject)
                .ToListAsync();

            foreach (var fk in fkDeps)
            {
                var node = new LineageNode
                {
                    ObjectName = fk.ChildTable,
                    ObjectType = await GetObjectTypeAsync(fk.ChildTable),
                    Depth = currentDepth + 1,
                    Path = $"{rootPath} → {fk.ChildTable}",
                    LinkType = "FK",
                    Metadata = new Dictionary<string, object>
                    {
                        ["ParentColumn"] = fk.ParentColumn,
                        ["ChildColumn"] = fk.ChildColumn
                    }
                };

                result.Add(node);
                await TraverseDownstreamAsync(fk.ChildTable, currentDepth + 1, maxDepth, visited, result, rootPath);
            }

            // Get SQL dependencies (this object as source)
            var sqlDeps = await _context.SqlDependencies
                .Where(sql => sql.SourceObject == currentObject)
                .ToListAsync();

            foreach (var sql in sqlDeps)
            {
                var node = new LineageNode
                {
                    ObjectName = sql.TargetObject,
                    ObjectType = sql.ObjectType,
                    Depth = currentDepth + 1,
                    Path = $"{rootPath} → {sql.TargetObject}",
                    LinkType = "SQL",
                };

                result.Add(node);
                await TraverseDownstreamAsync(sql.TargetObject, currentDepth + 1, maxDepth, visited, result, rootPath);
            }
        }

        private async Task<string> GetObjectTypeAsync(string objectName)
        {
            // Simple heuristic - in a real implementation, you'd query the database schema
            if (objectName.ToLower().Contains("view"))
                return "VIEW";
            if (objectName.ToLower().Contains("proc") || objectName.ToLower().Contains("sp_"))
                return "PROC";
            return "TABLE";
        }

        private async Task<int> GetTableRowCountAsync(string tableName)
        {
            var connectionString = "Data Source=New.sqlite";
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            var command = new SqliteCommand($"SELECT COUNT(*) FROM {tableName}", connection);
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        private async Task<DateTime?> GetTableLastRefreshAsync(string tableName)
        {
            // In a real implementation, you'd query your metadata table
            // For now, we'll return null to indicate it's not implemented
            return null;
        }

        public async Task<List<string>> GetAllObjectsAsync()
        {
            var fkObjects = await _context.ForeignKeyDependencies
                .Select(fk => fk.ParentTable)
                .Union(_context.ForeignKeyDependencies.Select(fk => fk.ChildTable))
                .ToListAsync();

            var sqlObjects = await _context.SqlDependencies
                .Select(sql => sql.SourceObject)
                .Union(_context.SqlDependencies.Select(sql => sql.TargetObject))
                .ToListAsync();

            return fkObjects.Union(sqlObjects).Distinct().OrderBy(x => x).ToList();
        }

        public async Task<List<ForeignKeyDependency>> GetAllForeignKeyDependenciesAsync()
        {
            return await _context.ForeignKeyDependencies.ToListAsync();
        }

        public async Task<List<SqlDependency>> GetAllSqlDependenciesAsync()
        {
            return await _context.SqlDependencies.ToListAsync();
        }
    }
}
