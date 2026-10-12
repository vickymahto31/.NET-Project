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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DatabaseHelper _db;
        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(DatabaseHelper db, ILogger<CategoryRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            if (!_db.IsSqlServerAvailable())
            {
                // Fallback store
                var mockList = DatabaseHelper.MockCategories.ToList();
                foreach (var c in mockList)
                {
                    c.PostCount = DatabaseHelper.MockPosts.Count(p => p.CategoryId == c.CategoryId && !p.IsHidden);
                }
                return mockList.OrderBy(c => c.CategoryId).ToList();
            }

            var categories = new List<Category>();
            string query = @"
                SELECT c.CategoryId, c.Name, c.Description, c.IconClass, c.BadgeColor,
                       COUNT(p.PostId) AS PostCount
                FROM Categories c
                LEFT JOIN Posts p ON c.CategoryId = p.CategoryId AND p.IsHidden = 0
                GROUP BY c.CategoryId, c.Name, c.Description, c.IconClass, c.BadgeColor
                ORDER BY c.CategoryId ASC";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(query, conn))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            categories.Add(new Category
                            {
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                IconClass = reader.GetString(reader.GetOrdinal("IconClass")),
                                BadgeColor = reader.GetString(reader.GetOrdinal("BadgeColor")),
                                PostCount = reader.GetInt32(reader.GetOrdinal("PostCount"))
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in CategoryRepository.GetAllCategoriesAsync. Falling back to local data.");
                return DatabaseHelper.MockCategories.OrderBy(c => c.CategoryId).ToList();
            }

            return categories;
        }

        public async Task<Category?> GetCategoryByIdAsync(int categoryId)
        {
            if (!_db.IsSqlServerAvailable())
            {
                return DatabaseHelper.MockCategories.FirstOrDefault(c => c.CategoryId == categoryId);
            }

            string query = @"
                SELECT CategoryId, Name, Description, IconClass, BadgeColor
                FROM Categories
                WHERE CategoryId = @CategoryId";

            try
            {
                using (var conn = _db.CreateConnection())
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value = categoryId;

                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Category
                            {
                                CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Description = reader.GetString(reader.GetOrdinal("Description")),
                                IconClass = reader.GetString(reader.GetOrdinal("IconClass")),
                                BadgeColor = reader.GetString(reader.GetOrdinal("BadgeColor"))
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ADO.NET Error in CategoryRepository.GetCategoryByIdAsync.");
                return DatabaseHelper.MockCategories.FirstOrDefault(c => c.CategoryId == categoryId);
            }

            return null;
        }
    }
}
