using System.Collections.Generic;
using System.Threading.Tasks;
using AnonymousSharingPlatform.Models;

namespace AnonymousSharingPlatform.Data.Repositories
{
    public interface IReportRepository
    {
        Task<int> CreateReportAsync(Report report);
        Task<List<Report>> GetAllReportsAsync(string? status = null);
        Task<Report?> GetReportByIdAsync(int reportId);
        Task<bool> UpdateReportStatusAsync(int reportId, string status);
        Task<int> GetPendingReportsCountAsync();
    }
}
