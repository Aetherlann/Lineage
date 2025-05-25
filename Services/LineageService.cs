using Microsoft.EntityFrameworkCore;
using MyDashboardApp.Data;
using MyDashboardApp.Models;
using Microsoft.Data.Sqlite;
using System.Linq; // Added for SanitizeTableName
using System.Data.Common; // Added for DbConnection/DbCommand
using System.Threading.Tasks; // Added for Task
using System; // Added for Exception, Convert
using System.Collections.Generic; // Added for Dictionary, HashSet, List

namespace MyDashboardApp.Services
{
    public class LineageService
    {
        private readonly NewDbLineageContext _context;

        public LineageService(NewDbLineageContext context)
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

            treeData.UpstreamNodes = await GetUpstreamLineageAsync(objectName, maxDepth);
            treeData.TotalUpstreamCount = treeData.UpstreamNodes.Count;
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
                    Metadata = new Dictionary<string, object> { ["ParentColumn"] = fk.ParentColumn, ["ChildColumn"] = fk.ChildColumn }
                };
                result.Add(node);
                await TraverseUpstreamAsync(fk.ParentTable, currentDepth + 1, maxDepth, visited, result, rootPath);
            }

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
                    Metadata = new Dictionary<string, object> { ["ParentColumn"] = fk.ParentColumn, ["ChildColumn"] = fk.ChildColumn }
                };
                result.Add(node);
                await TraverseDownstreamAsync(fk.ChildTable, currentDepth + 1, maxDepth, visited, result, rootPath);
            }

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
            var sqlDep = await _context.SqlDependencies
                                   .FirstOrDefaultAsync(s => s.SourceObject == objectName || s.TargetObject == objectName);
            if (sqlDep != null)
            {
                if (sqlDep.SourceObject == objectName && !string.IsNullOrEmpty(sqlDep.ObjectType))
                    return sqlDep.ObjectType;
                if (sqlDep.TargetObject == objectName && !string.IsNullOrEmpty(sqlDep.ObjectType))
                     return sqlDep.ObjectType; 
            }
            if (objectName.ToLower().Contains("view")) return "VIEW";
            if (objectName.ToLower().Contains("proc") || objectName.ToLower().Contains("sp_")) return "PROC";
            return "TABLE";
        }

        private string SanitizeTableName(string tableName)
        {
            // Basic sanitization: allow only letters, digits, underscores.
            // Adjust if other characters are valid and safe in your table names.
            return new string(tableName.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
        }

        private async Task<int> GetTableRowCountAsync(string tableName)
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                // Using DbCommand for broader compatibility if underlying provider changes
                using var command = connection.CreateCommand();
                var sanitizedTableName = SanitizeTableName(tableName);
                // It's generally safer to ensure table/column names are not from user input
                // or are from a known safe list, even with sanitization.
                if (string.IsNullOrWhiteSpace(sanitizedTableName)) 
                {
                    // Or throw an ArgumentException if an empty/invalid table name is critical
                    Console.WriteLine($"Sanitized table name for '{tableName}' is empty or whitespace.");
                    return 0; 
                }
                command.CommandText = $"SELECT COUNT(*) FROM \"{sanitizedTableName}\""; // Quoting for SQLite, adjust if needed for other DBs
                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                // Consider logging the exception (e.g., using ILogger if injected)
                Console.WriteLine($"Error in GetTableRowCountAsync for table {tableName}: {ex.Message}"); // Temporary logging
                return 0; 
            }
        }

        private async Task<DateTime?> GetTableLastRefreshAsync(string tableName)
        {
            // In a real implementation, you'd query your metadata table
            // This method would also benefit from using _context.Database.GetDbConnection()
            // and sanitization if it were to execute dynamic SQL.
            // For now, returning null as per original.
            return await Task.FromResult<DateTime?>(null);
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
