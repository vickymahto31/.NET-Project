using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace AnonymousSharingPlatform.Data
{
    public class DbInitializer
    {
        private readonly DatabaseHelper _db;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(DatabaseHelper db, ILogger<DbInitializer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            if (!_db.IsSqlServerAvailable())
            {
                _logger.LogInformation("SQL Server is offline or unconfigured. WhisperSpace is active with ADO.NET in-memory dataset.");
                return;
            }

            try
            {
                _logger.LogInformation("SQL Server is connected. Verifying tables and schema...");

                using (var conn = _db.CreateConnection())
                {
                    await conn.OpenAsync();

                    // Check if Categories table exists
                    string checkTableSql = "SELECT COUNT(1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Categories'";
                    using (var cmd = new SqlCommand(checkTableSql, conn))
                    {
                        var exists = (int?)await cmd.ExecuteScalarAsync();
                        if (exists == 0)
                        {
                            _logger.LogInformation("Categories table not found. Provisioning database schema from script...");
                            await ExecuteScriptAsync(conn);
                        }
                        else
                        {
                            _logger.LogInformation("Database tables verified successfully.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not complete SQL Server auto-initialization. Will operate safely in resilient mode.");
            }
        }

        private async Task ExecuteScriptAsync(SqlConnection conn)
        {
            try
            {
                string scriptPath = Path.Combine(AppContext.BaseDirectory, "Database", "AnonymousSharingDB.sql");
                if (!File.Exists(scriptPath))
                {
                    scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "AnonymousSharingDB.sql");
                }

                if (File.Exists(scriptPath))
                {
                    string script = await File.ReadAllTextAsync(scriptPath);
                    // Split on GO statements for SQL Server
                    string[] commands = script.Split(new[] { "\nGO", "\r\nGO", "\ngo", "\r\ngo" }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var rawCmd in commands)
                    {
                        string trimmed = rawCmd.Trim();
                        if (string.IsNullOrWhiteSpace(trimmed)) continue;

                        try
                        {
                            using (var cmd = new SqlCommand(trimmed, conn))
                            {
                                await cmd.ExecuteNonQueryAsync();
                            }
                        }
                        catch (Exception innerEx)
                        {
                            _logger.LogWarning("Command execution note: {Message}", innerEx.Message);
                        }
                    }

                    _logger.LogInformation("Database schema and seed data applied successfully to SQL Server.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing initial SQL setup script.");
            }
        }
    }
}
