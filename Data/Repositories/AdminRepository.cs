using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using AnonymousSharingPlatform.Models;

namespace AnonymousSharingPlatform.Data.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly DatabaseHelper _db;
        private readonly ILogger<AdminRepository> _logger;

        public AdminRepository(DatabaseHelper db, ILogger<AdminRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<AdminUser?> AuthenticateAsync(string username, string rawPassword)
        {
            string hash = ComputeSha256Hash(rawPassword);

            if (!_db.IsSqlServerAvailable())
            {
                var admin = DatabaseHelper.MockAdmins.FirstOrDefault(a => 
                    string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase) && 
                    string.Equals(a.PasswordHash, hash, StringComparison.OrdinalIgnoreCase));
                return admin;
            }

            string sql = @"
                SELECT AdminId, Username, PasswordHash, FullName, CreatedAt
                FROM Admins
                WHERE LOWER(Username) = LOWER(@Username) AND PasswordHash = @PasswordHash";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username.Trim() });
                    cmd.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, 256) { Value = hash });

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new AdminUser
                            {
                                AdminId = reader.GetInt32(reader.GetOrdinal("AdminId")),
                                Username = reader.GetString(reader.GetOrdinal("Username")),
                                PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
                                FullName = reader.GetString(reader.GetOrdinal("FullName")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in AdminRepository.AuthenticateAsync.");
                return DatabaseHelper.MockAdmins.FirstOrDefault(a => 
                    string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase) && 
                    string.Equals(a.PasswordHash, hash, StringComparison.OrdinalIgnoreCase));
            }

            return null;
        }

        public async Task<bool> ValidateSessionAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;

            if (!_db.IsSqlServerAvailable())
            {
                return DatabaseHelper.MockAdmins.Any(a => string.Equals(a.Username, username, StringComparison.OrdinalIgnoreCase));
            }

            string sql = @"SELECT COUNT(1) FROM Admins WHERE LOWER(Username) = LOWER(@Username)";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Username", SqlDbType.NVarChar, 50) { Value = username.Trim() });
                    await conn.OpenAsync();
                    var result = await cmd.ExecuteScalarAsync();
                    return result != null && Convert.ToInt32(result) > 0;
                }
            }
            catch
            {
                return true;
            }
        }

        public static string ComputeSha256Hash(string rawData)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                var builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
