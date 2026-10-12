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
    public class CommentRepository : ICommentRepository
    {
        private readonly DatabaseHelper _db;
        private readonly ILogger<CommentRepository> _logger;

        public CommentRepository(DatabaseHelper db, ILogger<CommentRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId, bool includeHidden = false)
        {
            if (!_db.IsSqlServerAvailable())
            {
                return DatabaseHelper.MockComments
                    .Where(c => c.PostId == postId && (includeHidden || !c.IsHidden))
                    .OrderBy(c => c.CreatedAt)
                    .ToList();
            }

            var comments = new List<Comment>();
            string sql = @"
                SELECT CommentId, PostId, AuthorAlias, Content, CreatedAt, IsHidden
                FROM Comments
                WHERE PostId = @PostId " + (includeHidden ? "" : " AND IsHidden = 0 ") + @"
                ORDER BY CreatedAt ASC";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = postId });
                    await conn.OpenAsync();

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            comments.Add(new Comment
                            {
                                CommentId = reader.GetInt32(reader.GetOrdinal("CommentId")),
                                PostId = reader.GetInt32(reader.GetOrdinal("PostId")),
                                AuthorAlias = reader.GetString(reader.GetOrdinal("AuthorAlias")),
                                Content = reader.GetString(reader.GetOrdinal("Content")),
                                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                                IsHidden = reader.GetBoolean(reader.GetOrdinal("IsHidden"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in CommentRepository.GetCommentsByPostIdAsync.");
                return DatabaseHelper.MockComments
                    .Where(c => c.PostId == postId && (includeHidden || !c.IsHidden))
                    .OrderBy(c => c.CreatedAt)
                    .ToList();
            }

            return comments;
        }

        public async Task<int> AddCommentAsync(Comment comment)
        {
            if (!_db.IsSqlServerAvailable())
            {
                comment.CommentId = DatabaseHelper.GetNextCommentId();
                comment.CreatedAt = DateTime.Now;
                DatabaseHelper.MockComments.Add(comment);

                var post = DatabaseHelper.MockPosts.FirstOrDefault(p => p.PostId == comment.PostId);
                if (post != null)
                {
                    post.CommentCount++;
                }

                return comment.CommentId;
            }

            string sql = @"
                INSERT INTO Comments (PostId, AuthorAlias, Content, CreatedAt, IsHidden)
                OUTPUT INSERTED.CommentId
                VALUES (@PostId, @AuthorAlias, @Content, GETDATE(), 0);";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@PostId", SqlDbType.Int) { Value = comment.PostId });
                    cmd.Parameters.Add(new SqlParameter("@AuthorAlias", SqlDbType.NVarChar, 80) { Value = string.IsNullOrWhiteSpace(comment.AuthorAlias) ? "Supportive Peer" : comment.AuthorAlias });
                    cmd.Parameters.Add(new SqlParameter("@Content", SqlDbType.NVarChar, 1500) { Value = comment.Content });

                    await conn.OpenAsync();
                    var insertedId = await cmd.ExecuteScalarAsync();
                    return insertedId != null ? Convert.ToInt32(insertedId) : 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in CommentRepository.AddCommentAsync.");
                comment.CommentId = DatabaseHelper.GetNextCommentId();
                DatabaseHelper.MockComments.Add(comment);
                return comment.CommentId;
            }
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            if (!_db.IsSqlServerAvailable())
            {
                var comment = DatabaseHelper.MockComments.FirstOrDefault(c => c.CommentId == commentId);
                if (comment != null)
                {
                    comment.IsHidden = true;
                    return true;
                }
                return false;
            }

            string sql = @"DELETE FROM Comments WHERE CommentId = @CommentId";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.Add(new SqlParameter("@CommentId", SqlDbType.Int) { Value = commentId });
                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in CommentRepository.DeleteCommentAsync.");
                return false;
            }
        }

        public async Task<int> GetTotalCommentsCountAsync()
        {
            if (!_db.IsSqlServerAvailable())
            {
                return DatabaseHelper.MockComments.Count(c => !c.IsHidden);
            }

            string sql = @"SELECT COUNT(1) FROM Comments WHERE IsHidden = 0";

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
                return DatabaseHelper.MockComments.Count(c => !c.IsHidden);
            }
        }
    }
}
