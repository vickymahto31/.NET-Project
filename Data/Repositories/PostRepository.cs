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
    public class PostRepository : IPostRepository
    {
        private readonly DatabaseHelper _db;
        private readonly ILogger<PostRepository> _logger;

        public PostRepository(DatabaseHelper db, ILogger<PostRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Post>> GetPostsAsync(int? categoryId = null, string? searchTerm = null, string? sortBy = null, string? mood = null, bool includeHidden = false)
        {
            if (!_db.IsSqlServerAvailable())
            {
                var query = DatabaseHelper.MockPosts.AsEnumerable();

                if (!includeHidden)
                    query = query.Where(p => !p.IsHidden);

                if (categoryId.HasValue && categoryId.Value > 0)
                    query = query.Where(p => p.CategoryId == categoryId.Value);

                if (!string.IsNullOrWhiteSpace(mood))
                    query = query.Where(p => string.Equals(p.MoodTag, mood, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLowerInvariant();
                    query = query.Where(p => p.Title.ToLowerInvariant().Contains(term) || p.Content.ToLowerInvariant().Contains(term));
                }

                query = sortBy?.ToLowerInvariant() switch
                {
                    "popular" => query.OrderByDescending(p => p.SupportCount).ThenByDescending(p => p.CreatedAt),
                    "discussed" => query.OrderByDescending(p => p.CommentCount).ThenByDescending(p => p.CreatedAt),
                    _ => query.OrderByDescending(p => p.CreatedAt)
                };

                return query.ToList();
            }

            var posts = new List<Post>();

            // Base SQL Query with ADO.NET
            string sql = @"
                SELECT p.PostId, p.Title, p.Content, p.CategoryId, p.MoodTag, p.AuthorAlias,
                       p.SupportCount, p.ReportCount, p.IsHidden, p.CreatedAt,
                       c.Name AS CategoryName, c.BadgeColor AS CategoryBadgeColor,
                       (SELECT COUNT(1) FROM Comments cm WHERE cm.PostId = p.PostId AND cm.IsHidden = 0) AS CommentCount
                FROM Posts p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE 1 = 1 ";

            var parameters = new List<SqlParameter>();

            if (!includeHidden)
            {
                sql += " AND p.IsHidden = 0 ";
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                sql += " AND p.CategoryId = @CategoryId ";
                parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = categoryId.Value });
            }

            if (!string.IsNullOrWhiteSpace(mood))
            {
                sql += " AND p.MoodTag = @MoodTag ";
                parameters.Add(new SqlParameter("@MoodTag", SqlDbType.NVarChar, 50) { Value = mood });
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                sql += " AND (p.Title LIKE @SearchTerm OR p.Content LIKE @SearchTerm) ";
                parameters.Add(new SqlParameter("@SearchTerm", SqlDbType.NVarChar, 200) { Value = $"%{searchTerm.Trim()}%" });
            }

            // Sorting
            sql += sortBy?.ToLowerInvariant() switch
            {
                "popular" => " ORDER BY p.SupportCount DESC, p.CreatedAt DESC ",
                "discussed" => " ORDER BY CommentCount DESC, p.CreatedAt DESC ",
                _ => " ORDER BY p.CreatedAt DESC "
            };

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
                            posts.Add(MapPostFromReader(reader));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in PostRepository.GetPostsAsync. Falling back to local data.");
                return DatabaseHelper.MockPosts.Where(p => includeHidden || !p.IsHidden).OrderByDescending(p => p.CreatedAt).ToList();
            }

            return posts;
        }

        public async Task<Post?> GetPostByIdAsync(int postId, bool includeHidden = false)
        {
            if (!_db.IsSqlServerAvailable())
            {
                var p = DatabaseHelper.MockPosts.FirstOrDefault(x => x.PostId == postId && (includeHidden || !x.IsHidden));
                if (p != null)
                {
                    p.CommentCount = DatabaseHelper.MockComments.Count(c => c.PostId == p.PostId && !c.IsHidden);
                }
                return p;
            }

            string sql = @"
                SELECT p.PostId, p.Title, p.Content, p.CategoryId, p.MoodTag, p.AuthorAlias,
                       p.SupportCount, p.ReportCount, p.IsHidden, p.CreatedAt,
                       c.Name AS CategoryName, c.BadgeColor AS CategoryBadgeColor,
                       (SELECT COUNT(1) FROM Comments cm WHERE cm.PostId = p.PostId AND cm.IsHidden = 0) AS CommentCount
                FROM Posts p
                INNER JOIN Categories c ON p.CategoryId = c.CategoryId
                WHERE p.PostId = @PostId " + (includeHidden ? "" : " AND p.IsHidden = 0 ");

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapPostFromReader(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in PostRepository.GetPostByIdAsync.");
                return DatabaseHelper.MockPosts.FirstOrDefault(x => x.PostId == postId);
            }

            return null;
        }

        public async Task<int> CreatePostAsync(Post post)
        {
            if (!_db.IsSqlServerAvailable())
            {
                post.PostId = DatabaseHelper.GetNextPostId();
                post.CreatedAt = DateTime.Now;
                var cat = DatabaseHelper.MockCategories.FirstOrDefault(c => c.CategoryId == post.CategoryId);
                if (cat != null)
                {
                    post.CategoryName = cat.Name;
                    post.CategoryBadgeColor = cat.BadgeColor;
                }
                DatabaseHelper.MockPosts.Add(post);
                return post.PostId;
            }

            string sql = @"
                INSERT INTO Posts (Title, Content, CategoryId, MoodTag, AuthorAlias, SupportCount, ReportCount, IsHidden, CreatedAt)
                OUTPUT INSERTED.PostId
                VALUES (@Title, @Content, @CategoryId, @MoodTag, @AuthorAlias, 0, 0, 0, GETDATE());";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 200) { Value = post.Title });
                    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.NVarChar, -1) { Value = post.Content });
                    cmd.Parameters.Add(new SqlParameter("@CategoryId", SqlDbType.Int) { Value = post.CategoryId });
                    cmd.Parameters.Add(new SqlParameter("@MoodTag", SqlDbType.NVarChar, 50) { Value = post.MoodTag });
                    cmd.Parameters.Add(new SqlParameter("@AuthorAlias", SqlDbType.NVarChar, 80) { Value = string.IsNullOrWhiteSpace(post.AuthorAlias) ? "Anonymous Student" : post.AuthorAlias });

                    await conn.OpenAsync();
                    var insertedId = await cmd.ExecuteScalarAsync();
                    return insertedId != null ? Convert.ToInt32(insertedId) : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in PostRepository.CreatePostAsync.");
                // Fallback
                post.PostId = DatabaseHelper.GetNextPostId();
                DatabaseHelper.MockPosts.Add(post);
                return post.PostId;
            }
        }

        public async Task<bool> IncrementSupportAsync(int postId)
        {
            if (!_db.IsSqlServerAvailable())
            {
                var post = DatabaseHelper.MockPosts.FirstOrDefault(p => p.PostId == postId);
                if (post != null)
                {
                    post.SupportCount++;
                    return true;
                }
                return false;
            }

            string sql = @"UPDATE Posts SET SupportCount = SupportCount + 1 WHERE PostId = @PostId";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in PostRepository.IncrementSupportAsync.");
                var post = DatabaseHelper.MockPosts.FirstOrDefault(p => p.PostId == postId);
                if (post != null) { post.SupportCount++; return true; }
                return false;
            }
        }

        public async Task<bool> IncrementReportCountAsync(int postId)
        {
            if (!_db.IsSqlServerAvailable())
            {
                var post = DatabaseHelper.MockPosts.FirstOrDefault(p => p.PostId == postId);
                if (post != null)
                {
                    post.ReportCount++;
                    return true;
                }
                return false;
            }

            string sql = @"UPDATE Posts SET ReportCount = ReportCount + 1 WHERE PostId = @PostId";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in PostRepository.IncrementReportCountAsync.");
                var post = DatabaseHelper.MockPosts.FirstOrDefault(p => p.PostId == postId);
                if (post != null) { post.ReportCount++; return true; }
                return false;
            }
        }

        public async Task<bool> ToggleHidePostAsync(int postId, bool isHidden)
        {
            if (!_db.IsSqlServerAvailable())
            {
                var post = DatabaseHelper.MockPosts.FirstOrDefault(p => p.PostId == postId);
                if (post != null)
                {
                    post.IsHidden = isHidden;
                    return true;
                }
                return false;
            }

            string sql = @"UPDATE Posts SET IsHidden = @IsHidden WHERE PostId = @PostId";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@IsHidden", SqlDbType.Bit) { Value = isHidden });
                    cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in PostRepository.ToggleHidePostAsync.");
                var post = DatabaseHelper.MockPosts.FirstOrDefault(p => p.PostId == postId);
                if (post != null) { post.IsHidden = isHidden; return true; }
                return false;
            }
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            if (!_db.IsSqlServerAvailable())
            {
                var p = DatabaseHelper.MockPosts.FirstOrDefault(x => x.PostId == postId);
                if (p != null)
                {
                    p.IsHidden = true; // Soft delete or remove
                    return true;
                }
                return false;
            }

            string sql = @"DELETE FROM Posts WHERE PostId = @PostId";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in PostRepository.DeletePostAsync.");
                return false;
            }
        }

        public async Task<int> GetTotalPostsCountAsync()
        {
            if (!_db.IsSqlServerAvailable())
            {
                return DatabaseHelper.MockPosts.Count(p => !p.IsHidden);
            }

            string sql = @"SELECT COUNT(1) FROM Posts WHERE IsHidden = 0";

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
                return DatabaseHelper.MockPosts.Count(p => !p.IsHidden);
            }
        }

        private static Post MapPostFromReader(SqlDataReader reader)
        {
            return new Post
            {
                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Content = reader.GetString(reader.GetOrdinal("Content")),
                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                MoodTag = reader.GetString(reader.GetOrdinal("MoodTag")),
                AuthorAlias = reader.GetString(reader.GetOrdinal("AuthorAlias")),
                SupportCount = reader.GetInt32(reader.GetOrdinal("SupportCount")),
                ReportCount = reader.GetInt32(reader.GetOrdinal("ReportCount")),
                IsHidden = reader.GetBoolean(reader.GetOrdinal("IsHidden")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                CategoryBadgeColor = reader.GetString(reader.GetOrdinal("CategoryBadgeColor")),
                CommentCount = reader.GetInt32(reader.GetOrdinal("CommentCount"))
            };
        }
    }
}
