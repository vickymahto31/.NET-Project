using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using AnonymousSharingPlatform.Models;

namespace AnonymousSharingPlatform.Data.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly DatabaseHelper _db;
        private readonly IPostRepository _postRepo;
        private readonly ILogger<ReportRepository> _logger;

        public ReportRepository(DatabaseHelper db, IPostRepository postRepo, ILogger<ReportRepository> logger)
        {
            _db = db;
            _postRepo = postRepo;
            _logger = logger;
        }

        public async Task<int> CreateReportAsync(Report report)
        {
            // First increment report count on the post
            await _postRepo.IncrementReportCountAsync(report.PostId);

            if (!_db.IsSqlServerAvailable())
            {
                report.ReportId = DatabaseHelper.GetNextReportId();
                report.CreatedAt = DateTime.Now;
                report.Status = "Pending";

                var post = DatabaseHelper.MockPosts.FirstOrDefault(p => p.PostId == report.PostId);
                if (post != null)
                {
                    report.PostTitle = post.Title;
                    report.PostContentSnippet = post.Content.Length > 100 ? post.Content.Substring(0, 100) + "..." : post.Content;
                }

                DatabaseHelper.MockReports.Add(report);
                return report.ReportId;
            }

            string sql = @"
                INSERT INTO Reports (PostId, Reason, DetailedRemarks, Status, CreatedAt)
                OUTPUT INSERTED.ReportId
                VALUES (@PostId, @Reason, @DetailedRemarks, 'Pending', GETDATE());";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = report.PostId });
                    cmd.Parameters.Add(new SqlParameter("@Reason", SqlDbType.NVarChar, 100) { Value = report.Reason });
                    cmd.Parameters.Add(new SqlParameter("@DetailedRemarks", SqlDbType.NVarChar, 500) { Value = (object?)report.DetailedRemarks ?? DBNull.Value });

                    await conn.OpenAsync();
                    var insertedId = await cmd.ExecuteScalarAsync();
                    return insertedId != null ? Convert.ToInt32(insertedId) : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in ReportRepository.CreateReportAsync.");
                report.ReportId = DatabaseHelper.GetNextReportId();
                DatabaseHelper.MockReports.Add(report);
                return report.ReportId;
            }
        }

        public async Task<List<Report>> GetAllReportsAsync(string? status = null)
        {
            if (!_db.IsSqlServerAvailable())
            {
                var query = DatabaseHelper.MockReports.AsEnumerable();
                if (!string.IsNullOrWhiteSpace(status))
                {
                    query = query.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase));
                }

                var list = query.OrderByDescending(r => r.CreatedAt).ToList();
                foreach (var r in list)
                {
                    var post = DatabaseHelper.MockPosts.FirstOrDefault(p => p.PostId == r.PostId);
                    if (post != null)
                    {
                        r.PostTitle = post.Title;
                        r.PostContentSnippet = post.Content.Length > 100 ? post.Content.Substring(0, 100) + "..." : post.Content;
                    }
                }
                return list;
            }

            var reports = new List<Report>();
            string sql = @"
                SELECT r.ReportId, r.PostId, r.Reason, r.DetailedRemarks, r.Status, r.CreatedAt,
                       p.Title AS PostTitle, SUBSTRING(p.Content, 1, 120) AS PostContentSnippet
                FROM Reports r
                INNER JOIN Posts p ON r.PostId = p.PostId
                WHERE 1 = 1 ";

            var parameters = new List<SqlParameter>();
            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND r.Status = @Status ";
                parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = status });
            }

            sql += " ORDER BY r.CreatedAt DESC";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reports.Add(new Report
                            {
                                ReportId = reader.GetInt32(reader.GetOrdinal("ReportId")),
                                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                                Reason = reader.GetString(reader.GetOrdinal("Reason")),
                                DetailedRemarks = reader.IsDBNull(reader.GetOrdinal("DetailedRemarks")) ? null : reader.GetString(reader.GetOrdinal("DetailedRemarks")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                PostTitle = reader.GetString(reader.GetOrdinal("PostTitle")),
                                PostContentSnippet = reader.GetString(reader.GetOrdinal("PostContentSnippet"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in ReportRepository.GetAllReportsAsync.");
                return DatabaseHelper.MockReports.OrderByDescending(r => r.CreatedAt).ToList();
            }

            return reports;
        }

        public async Task<Report?> GetReportByIdAsync(int reportId)
        {
            if (!_db.IsSqlServerAvailable())
            {
                return DatabaseHelper.MockReports.FirstOrDefault(r => r.ReportId == reportId);
            }

            string sql = @"
                SELECT r.ReportId, r.PostId, r.Reason, r.DetailedRemarks, r.Status, r.CreatedAt,
                       p.Title AS PostTitle, SUBSTRING(p.Content, 1, 120) AS PostContentSnippet
                FROM Reports r
                INNER JOIN Posts p ON r.PostId = p.PostId
                WHERE r.ReportId = @ReportId";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@ReportId", SqlDbType.Int) { Value = reportId });
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Report
                            {
                                ReportId = reader.GetInt32(reader.GetOrdinal("ReportId")),
                                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                                Reason = reader.GetString(reader.GetOrdinal("Reason")),
                                DetailedRemarks = reader.IsDBNull(reader.GetOrdinal("DetailedRemarks")) ? null : reader.GetString(reader.GetOrdinal("DetailedRemarks")),
                                Status = reader.GetString(reader.GetOrdinal("Status")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                PostTitle = reader.GetString(reader.GetOrdinal("PostTitle")),
                                PostContentSnippet = reader.GetString(reader.GetOrdinal("PostContentSnippet"))
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in ReportRepository.GetReportByIdAsync.");
                return DatabaseHelper.MockReports.FirstOrDefault(r => r.ReportId == reportId);
            }

            return null;
        }

        public async Task<bool> UpdateReportStatusAsync(int reportId, string status)
        {
            if (!_db.IsSqlServerAvailable())
            {
                var r = DatabaseHelper.MockReports.FirstOrDefault(x => x.ReportId == reportId);
                if (r != null)
                {
                    r.Status = status;
                    return true;
                }
                return false;
            }

            string sql = @"UPDATE Reports SET Status = @Status WHERE ReportId = @ReportId";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50) { Value = status });
                    cmd.Parameters.Add(new SqlParameter("@ReportId", SqlDbType.Int) { Value = reportId });

                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in ReportRepository.UpdateReportStatusAsync.");
                var r = DatabaseHelper.MockReports.FirstOrDefault(x => x.ReportId == reportId);
                if (r != null) { r.Status = status; return true; }
                return false;
            }
        }

        public async Task<int> GetPendingReportsCountAsync()
        {
            if (!_db.IsSqlServerAvailable())
            {
                return DatabaseHelper.MockReports.Count(r => r.Status == "Pending");
            }

            string sql = @"SELECT COUNT(1) FROM Reports WHERE Status = 'Pending'";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    await conn.OpenAsync();
                    var count = await cmd.ExecuteScalarAsync();
                    return count != null ? Convert.ToInt32(count) : 0;
                }
            }
            catch
            {
                return DatabaseHelper.MockReports.Count(r => r.Status == "Pending");
            }
        }
    }
}
