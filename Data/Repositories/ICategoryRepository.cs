using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousSharingPlatform.Models;

namespace AnonymousSharingPlatform.Data.Repositories
{
    public interface ICategoryRepository
    {
        Task<List<Category>> GetAllCategoriesAsync();
        Task<Category?> GetCategoryByIdAsync(int categoryId);
    }
}
