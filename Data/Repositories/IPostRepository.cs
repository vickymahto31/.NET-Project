using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousSharingPlatform.Models;

namespace AnonymousSharingPlatform.Data.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetPostsAsync(int? categoryId = null, string? searchTerm = null, string? sortBy = null, string? mood = null, bool includeHidden = false);
        Task<Post?> GetPostByIdAsync(int postId, bool includeHidden = false);
        Task<int> CreatePostAsync(Post post);
        Task<bool> IncrementSupportAsync(int postId);
        Task<bool> IncrementReportCountAsync(int postId);
        Task<bool> ToggleHidePostAsync(int postId, bool isHidden);
        Task<bool> DeletePostAsync(int postId);
        Task<int> GetTotalPostsCountAsync();
    }
}
