using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AnonymousSharingPlatform.Data.Repositories;
using AnonymousSharingPlatform.Models;
using AnonymousSharingPlatform.Models.ViewModels;

namespace AnonymousSharingPlatform.Controllers
{
    public class AdminController : Controller
    {
        private readonly IAdminRepository _adminRepo;
        private readonly IReportRepository _reportRepo;
        private readonly IPostRepository _postRepo;
        private readonly ICommentRepository _commentRepo;

        public AdminController(
            IAdminRepository adminRepo,
            IReportRepository reportRepo,
            IPostRepository postRepo,
            ICommentRepository commentRepo)
        {
            _adminRepo = adminRepo;
            _reportRepo = reportRepo;
            _postRepo = postRepo;
            _commentRepo = commentRepo;
        }

        private bool IsAdminAuthenticated()
        {
            return HttpContext.Session.GetString("AdminUsername") != null;
        }

        // GET: /Admin/Login
        public IActionResult Login()
        {
            if (IsAdminAuthenticated())
            {
                return RedirectToAction(nameof(Index));
            }
            return View(new LoginViewModel());
        }

        // POST: /Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var admin = await _adminRepo.AuthenticateAsync(model.Username, model.Password);
            if (admin == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid administrator credentials. Note: Default is admin / Admin@123");
                return View(model);
            }

            HttpContext.Session.SetString("AdminUsername", admin.Username);
            HttpContext.Session.SetString("AdminFullName", admin.FullName);

            TempData["SuccessMessage"] = $"Welcome back, {admin.FullName}! Moderation console loaded.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["InfoMessage"] = "You have been logged out of the moderation dashboard.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Admin/Index
        public async Task<IActionResult> Index()
        {
            if (!IsAdminAuthenticated())
            {
                TempData["ErrorMessage"] = "Please sign in to access the administrator moderation dashboard.";
                return RedirectToAction(nameof(Login));
            }

            var allPosts = await _postRepo.GetPostsAsync(includeHidden: true);
            var reports = await _reportRepo.GetAllReportsAsync();
            var totalComments = await _commentRepo.GetTotalCommentsCountAsync();
            var pendingReports = await _reportRepo.GetPendingReportsCountAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalPosts = allPosts.Count,
                TotalComments = totalComments,
                TotalReports = reports.Count,
                PendingReports = pendingReports,
                HiddenPosts = allPosts.Count(p => p.IsHidden),
                Reports = reports,
                AllPosts = allPosts
            };

            return View(viewModel);
        }

        // POST: /Admin/ToggleHidePost/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleHidePost(int postId, bool hide)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction(nameof(Login));

            await _postRepo.ToggleHidePostAsync(postId, hide);
            TempData["SuccessMessage"] = hide ? $"Post #{postId} has been hidden from public feed." : $"Post #{postId} is now visible on feed.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/DeletePost/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int postId)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction(nameof(Login));

            await _postRepo.DeletePostAsync(postId);
            TempData["SuccessMessage"] = $"Post #{postId} was deleted.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/UpdateReportStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateReportStatus(int reportId, string status)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction(nameof(Login));

            await _reportRepo.UpdateReportStatusAsync(reportId, status);
            TempData["SuccessMessage"] = $"Report #{reportId} marked as '{status}'.";
            return RedirectToAction(nameof(Index));
        }
    }
}
