using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousSharingPlatform.Models;

namespace AnonymousSharingPlatform.Data.Repositories
{
    public interface ICommentRepository
    {
        Task<List<Comment>> GetCommentsByPostIdAsync(int postId, bool includeHidden = false);
        Task<int> AddCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int commentId);
        Task<int> GetTotalCommentsCountAsync();
    }
}
