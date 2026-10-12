using System.Threading.Tasks;
using AnonymousSharingPlatform.Models;

namespace AnonymousSharingPlatform.Data.Repositories
{
    public interface IAdminRepository
    {
        Task<AdminUser?> AuthenticateAsync(string username, string rawPassword);
        Task<bool> ValidateSessionAsync(string username);
    }
}
