using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AnonymousSharingPlatform.Data.Repositories;
using AnonymousSharingPlatform.Models;
using AnonymousSharingPlatform.Models.ViewModels;

namespace AnonymousSharingPlatform.Controllers
{
    public class PostsController : Controller
    {
        private readonly IPostRepository _postRepo;
        private readonly ICategoryRepository _categoryRepo;
        private readonly ICommentRepository _commentRepo;
        private readonly IReportRepository _reportRepo;

        public PostsController(
            IPostRepository postRepo,
            ICategoryRepository categoryRepo,
            ICommentRepository commentRepo,
            IReportRepository reportRepo)
        {
            _postRepo = postRepo;
            _categoryRepo = categoryRepo;
            _commentRepo = commentRepo;
            _reportRepo = reportRepo;
        }

        // GET: /Posts or /Posts?categoryId=1&search=exam&sort=popular&mood=Overwhelmed
        public async Task<IActionResult> Index(int? categoryId, string? search, string? sort, string? mood)
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            var posts = await _postRepo.GetPostsAsync(categoryId, search, sort, mood);

            var viewModel = new FeedViewModel
            {
                Posts = posts,
                Categories = categories,
                SelectedCategoryId = categoryId,
                SelectedCategoryName = categoryId.HasValue ? categories.FirstOrDefault(c => c.CategoryId == categoryId.Value)?.Name : null,
                SearchTerm = search,
                SortBy = sort ?? "latest",
                SelectedMood = mood,
                TotalPostsCount = posts.Count
            };

            return View(viewModel);
        }

        // GET: /Posts/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var post = await _postRepo.GetPostByIdAsync(id);
            if (post == null)
            {
                TempData["ErrorMessage"] = "The requested post could not be found or has been removed.";
                return RedirectToAction(nameof(Index));
            }

            var comments = await _commentRepo.GetCommentsByPostIdAsync(id);

            var viewModel = new PostDetailsViewModel
            {
                Post = post,
                Comments = comments,
                NewComment = new Comment { PostId = id, AuthorAlias = "Supportive Peer" },
                NewReport = new Report { PostId = id }
            };

            return View(viewModel);
        }

        // GET: /Posts/Create
        public async Task<IActionResult> Create(int? categoryId)
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();

            var viewModel = new CreatePostViewModel
            {
                CategoryId = categoryId ?? (categories.FirstOrDefault()?.CategoryId ?? 1),
                AuthorAlias = GenerateRandomPseudonym(),
                MoodTag = "Venting",
                CategoryOptions = categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.Name
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: /Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostViewModel model)
        {
            var categories = await _categoryRepo.GetAllCategoriesAsync();
            model.CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.Name
            }).ToList();

            if (!model.AcceptCommunityRules)
            {
                ModelState.AddModelError(nameof(model.AcceptCommunityRules), "You must accept the community guidelines before posting.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var post = new Post
            {
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                CategoryId = model.CategoryId,
                MoodTag = model.MoodTag,
                AuthorAlias = string.IsNullOrWhiteSpace(model.AuthorAlias) ? "Anonymous Student" : model.AuthorAlias.Trim(),
                SupportCount = 0,
                ReportCount = 0,
                IsHidden = false,
                CreatedAt = DateTime.Now
            };

            int newPostId = await _postRepo.CreatePostAsync(post);

            TempData["SuccessMessage"] = "Your thought has been shared anonymously with the community.";
            return RedirectToAction(nameof(Details), new { id = newPostId });
        }

        // POST: /Posts/Support/5
        [HttpPost]
        public async Task<IActionResult> Support(int id)
        {
            await _postRepo.IncrementSupportAsync(id);
            TempData["SuccessMessage"] = "Support heart added! Your encouragement matters.";

            // Return to previous page or post details
            string? referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Posts/Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Report(ReportPostViewModel model)
        {
            if (model.PostId <= 0 || string.IsNullOrWhiteSpace(model.Reason))
            {
                TempData["ErrorMessage"] = "Please select a valid reason to report the post.";
                return RedirectToAction(nameof(Details), new { id = model.PostId });
            }

            var report = new Report
            {
                PostId = model.PostId,
                Reason = model.Reason,
                DetailedRemarks = model.DetailedRemarks?.Trim(),
                Status = "Pending"
            };

            await _reportRepo.CreateReportAsync(report);

            TempData["SuccessMessage"] = "Thank you. This post has been reported and escalated to moderation.";
            return RedirectToAction(nameof(Details), new { id = model.PostId });
        }

        private static string GenerateRandomPseudonym()
        {
            string[] adjectives = { "Quiet", "Curious", "Silent", "Hopeful", "Tired", "Gentle", "Brave", "Thoughtful", "Wandering", "Calm" };
            string[] nouns = { "Phoenix", "Owl", "Penguin", "Sparrow", "Nomad", "Seeker", "Anchor", "Breeze", "Voyager", "Dreamer" };
            var rnd = new Random();
            return $"{adjectives[rnd.Next(adjectives.Length)]} {nouns[rnd.Next(nouns.Length)]}";
        }
    }
}
