using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MyDashboardApp.Data;
using MyDashboardApp.Models;

namespace MyDashboardApp.Services
{
    public class DataLoadingService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public DataLoadingService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task LoadLineageDataAsync()
        {
            try
            {
                // Clear existing data
                _context.ForeignKeyDependencies.RemoveRange(_context.ForeignKeyDependencies);
                _context.SqlDependencies.RemoveRange(_context.SqlDependencies);
                await _context.SaveChangesAsync();

                // Load data from New.sqlite
                await LoadForeignKeyDependenciesAsync();
                await LoadSqlDependenciesAsync();

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log error - in production you'd use proper logging
                Console.WriteLine($"Error loading lineage data: {ex.Message}");
                throw;
            }
        }

        private async Task LoadForeignKeyDependenciesAsync()
        {
            var newSqliteConnectionString = "Data Source=New.sqlite";
            
            using var connection = new SqliteConnection(newSqliteConnectionString);
            await connection.OpenAsync();

            // First, let's check what tables exist in New.sqlite
            var tablesQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name LIKE 'Q%' ORDER BY name";
            using var tablesCommand = new SqliteCommand(tablesQuery, connection);
            using var tablesReader = await tablesCommand.ExecuteReaderAsync();
            
            var tables = new List<string>();
            while (await tablesReader.ReadAsync())
            {
                tables.Add(tablesReader.GetString(0));
            }
            tablesReader.Close();

            // Look for foreign key data - try common table names that might contain FK info
            foreach (var tableName in tables)
            {
                try
                {
                    // Get column info for this table
                    var columnsQuery = $"PRAGMA table_info({tableName})";
                    using var columnsCommand = new SqliteCommand(columnsQuery, connection);
                    using var columnsReader = await columnsCommand.ExecuteReaderAsync();
                    
                    var columns = new List<string>();
                    while (await columnsReader.ReadAsync())
                    {
                        columns.Add(columnsReader.GetString(1)); // Column name is at index 1
                    }
                    columnsReader.Close();

                    // Check if this table looks like it contains FK data
                    if (HasForeignKeyColumns(columns))
                    {
                        await LoadForeignKeyDataFromTable(connection, tableName, columns);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing table {tableName}: {ex.Message}");
                    // Continue with next table
                }
            }
        }

        private bool HasForeignKeyColumns(List<string> columns)
        {
            // Look for column patterns that suggest this is FK data
            var lowerColumns = columns.Select(c => c.ToLower()).ToList();
            
            return (lowerColumns.Contains("fk_table") && lowerColumns.Contains("referenced_table")) ||
                   (lowerColumns.Contains("parent_table") && lowerColumns.Contains("child_table")) ||
                   (lowerColumns.Contains("table_name") && lowerColumns.Contains("referenced_table_name"));
        }

        private async Task LoadForeignKeyDataFromTable(SqliteConnection connection, string tableName, List<string> columns)
        {
            var query = $"SELECT * FROM {tableName} LIMIT 100"; // Limit for safety
            using var command = new SqliteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var columnMap = MapForeignKeyColumns(columns);
            
            while (await reader.ReadAsync())
            {
                try
                {
                    var fkDep = new ForeignKeyDependency
                    {
                        ParentTable = GetColumnValue(reader, columnMap["ParentTable"]) ?? "Unknown",
                        ChildTable = GetColumnValue(reader, columnMap["ChildTable"]) ?? "Unknown",
                        ParentColumn = GetColumnValue(reader, columnMap["ParentColumn"]) ?? "Unknown",
                        ChildColumn = GetColumnValue(reader, columnMap["ChildColumn"]) ?? "Unknown"
                    };

                    _context.ForeignKeyDependencies.Add(fkDep);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing FK row: {ex.Message}");
                    // Continue with next row
                }
            }
        }

        private Dictionary<string, string> MapForeignKeyColumns(List<string> columns)
        {
            var map = new Dictionary<string, string>();
            var lowerColumns = columns.ToDictionary(c => c.ToLower(), c => c);

            // Map common column name patterns to our model properties
            map["ParentTable"] = lowerColumns.ContainsKey("fk_table") ? lowerColumns["fk_table"] :
                               lowerColumns.ContainsKey("parent_table") ? lowerColumns["parent_table"] :
                               lowerColumns.ContainsKey("table_name") ? lowerColumns["table_name"] :
                               columns.FirstOrDefault() ?? "";

            map["ChildTable"] = lowerColumns.ContainsKey("referenced_table") ? lowerColumns["referenced_table"] :
                              lowerColumns.ContainsKey("child_table") ? lowerColumns["child_table"] :
                              lowerColumns.ContainsKey("referenced_table_name") ? lowerColumns["referenced_table_name"] :
                              columns.Skip(1).FirstOrDefault() ?? "";

            map["ParentColumn"] = lowerColumns.ContainsKey("fk_column") ? lowerColumns["fk_column"] :
                                lowerColumns.ContainsKey("parent_column") ? lowerColumns["parent_column"] :
                                lowerColumns.ContainsKey("column_name") ? lowerColumns["column_name"] :
                                columns.Skip(2).FirstOrDefault() ?? "";

            map["ChildColumn"] = lowerColumns.ContainsKey("referenced_column") ? lowerColumns["referenced_column"] :
                               lowerColumns.ContainsKey("child_column") ? lowerColumns["child_column"] :
                               lowerColumns.ContainsKey("referenced_column_name") ? lowerColumns["referenced_column_name"] :
                               columns.Skip(3).FirstOrDefault() ?? "";

            return map;
        }

        private async Task LoadSqlDependenciesAsync()
        {
            var newSqliteConnectionString = "Data Source=New.sqlite";
            
            using var connection = new SqliteConnection(newSqliteConnectionString);
            await connection.OpenAsync();

            // Get all tables
            var tablesQuery = "SELECT name FROM sqlite_master WHERE type='table' AND name LIKE 'Q%' ORDER BY name";
            using var tablesCommand = new SqliteCommand(tablesQuery, connection);
            using var tablesReader = await tablesCommand.ExecuteReaderAsync();
            
            var tables = new List<string>();
            while (await tablesReader.ReadAsync())
            {
                tables.Add(tablesReader.GetString(0));
            }
            tablesReader.Close();

            // Look for SQL dependency data
            foreach (var tableName in tables)
            {
                try
                {
                    // Get column info for this table
                    var columnsQuery = $"PRAGMA table_info({tableName})";
                    using var columnsCommand = new SqliteCommand(columnsQuery, connection);
                    using var columnsReader = await columnsCommand.ExecuteReaderAsync();
                    
                    var columns = new List<string>();
                    while (await columnsReader.ReadAsync())
                    {
                        columns.Add(columnsReader.GetString(1));
                    }
                    columnsReader.Close();

                    // Check if this table looks like it contains SQL dependency data
                    if (HasSqlDependencyColumns(columns))
                    {
                        await LoadSqlDependencyDataFromTable(connection, tableName, columns);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing table {tableName}: {ex.Message}");
                }
            }
        }

        private bool HasSqlDependencyColumns(List<string> columns)
        {
            var lowerColumns = columns.Select(c => c.ToLower()).ToList();
            
            return (lowerColumns.Contains("referencing_object") && lowerColumns.Contains("referenced_entity")) ||
                   (lowerColumns.Contains("source_object") && lowerColumns.Contains("target_object")) ||
                   (lowerColumns.Contains("object_name") && lowerColumns.Contains("referenced_object"));
        }

        private async Task LoadSqlDependencyDataFromTable(SqliteConnection connection, string tableName, List<string> columns)
        {
            var query = $"SELECT * FROM {tableName} LIMIT 100"; // Limit for safety
            using var command = new SqliteCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var columnMap = MapSqlDependencyColumns(columns);
            
            while (await reader.ReadAsync())
            {
                try
                {
                    var sqlDep = new SqlDependency
                    {
                        SourceObject = GetColumnValue(reader, columnMap["SourceObject"]) ?? "Unknown",
                        TargetObject = GetColumnValue(reader, columnMap["TargetObject"]) ?? "Unknown",
                        ObjectType = GetColumnValue(reader, columnMap["ObjectType"]) ?? "TABLE",
                        SqlTextSnippet = GetColumnValue(reader, columnMap["SqlTextSnippet"]) ?? ""
                    };

                    _context.SqlDependencies.Add(sqlDep);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing SQL dependency row: {ex.Message}");
                }
            }
        }

        private Dictionary<string, string> MapSqlDependencyColumns(List<string> columns)
        {
            var map = new Dictionary<string, string>();
            var lowerColumns = columns.ToDictionary(c => c.ToLower(), c => c);

            map["SourceObject"] = lowerColumns.ContainsKey("referencing_object") ? lowerColumns["referencing_object"] :
                                lowerColumns.ContainsKey("source_object") ? lowerColumns["source_object"] :
                                lowerColumns.ContainsKey("object_name") ? lowerColumns["object_name"] :
                                columns.FirstOrDefault() ?? "";

            map["TargetObject"] = lowerColumns.ContainsKey("referenced_entity") ? lowerColumns["referenced_entity"] :
                                lowerColumns.ContainsKey("target_object") ? lowerColumns["target_object"] :
                                lowerColumns.ContainsKey("referenced_object") ? lowerColumns["referenced_object"] :
                                columns.Skip(1).FirstOrDefault() ?? "";

            map["ObjectType"] = lowerColumns.ContainsKey("referencing_type") ? lowerColumns["referencing_type"] :
                              lowerColumns.ContainsKey("object_type") ? lowerColumns["object_type"] :
                              lowerColumns.ContainsKey("type") ? lowerColumns["type"] :
                              columns.Skip(2).FirstOrDefault() ?? "";

            map["SqlTextSnippet"] = lowerColumns.ContainsKey("sql_text") ? lowerColumns["sql_text"] :
                                  lowerColumns.ContainsKey("definition") ? lowerColumns["definition"] :
                                  lowerColumns.ContainsKey("snippet") ? lowerColumns["snippet"] :
                                  "";

            return map;
        }

        private string? GetColumnValue(SqliteDataReader reader, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                return null;

            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch
            {
                return null;
            }
        }
    }
}
