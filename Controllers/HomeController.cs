using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AnonymousSharingPlatform.Data.Repositories;
using AnonymousSharingPlatform.Models;
using AnonymousSharingPlatform.Models.ViewModels;

namespace AnonymousSharingPlatform.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;
        private readonly IPostRepository _postRepo;
        private readonly ICommentRepository _commentRepo;

        public HomeController(ICategoryRepository categoryRepo, IPostRepository postRepo, ICommentRepository commentRepo)
        {
            _categoryRepo = categoryRepo;
            _postRepo = postRepo;
            _commentRepo = commentRepo;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            var recentPosts = await _postRepo.GetPostsAsync();
            var totalPosts = await _postRepo.GetTotalPostsCountAsync();
            var totalComments = await _commentRepo.GetTotalCommentsCountAsync();

            ViewBag.TotalPosts = totalPosts;
            ViewBag.TotalComments = totalComments;
            ViewBag.TotalSupports = recentPosts.Sum(p => p.SupportCount);
            ViewBag.Categories = categories;

            // Pass top 4 recent posts to landing page
            return View(recentPosts.Take(4).ToList());
        }

        public IActionResult Guidelines()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
