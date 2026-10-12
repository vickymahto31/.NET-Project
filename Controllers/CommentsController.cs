using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AnonymousSharingPlatform.Data.Repositories;
using AnonymousSharingPlatform.Models;

namespace AnonymousSharingPlatform.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ICommentRepository _commentRepo;
        private readonly IPostRepository _postRepo;

        public CommentsController(ICommentRepository commentRepo, IPostRepository postRepo)
        {
            _commentRepo = commentRepo;
            _postRepo = postRepo;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int postId, string content, string? authorAlias)
        {
            if (postId <= 0)
            {
                TempData["ErrorMessage"] = "Invalid post.";
                return RedirectToAction("Index", "Posts");
            }

            if (string.IsNullOrWhiteSpace(content) || content.Trim().Length < 2)
            {
                TempData["ErrorMessage"] = "Comment cannot be empty.";
                return RedirectToAction("Details", "Posts", new { id = postId });
            }

            var post = await _postRepo.GetPostByIdAsync(postId);
            if (post == null)
            {
                TempData["ErrorMessage"] = "The post no longer exists.";
                return RedirectToAction("Index", "Posts");
            }

            var comment = new Comment
            {
                PostId = postId,
                AuthorAlias = string.IsNullOrWhiteSpace(authorAlias) ? "Supportive Peer" : authorAlias.Trim(),
                Content = content.Trim(),
                CreatedAt = DateTime.Now,
                IsHidden = false
            };

            await _commentRepo.AddCommentAsync(comment);

            TempData["SuccessMessage"] = "Your supportive comment has been added anonymously.";
            return RedirectToAction("Details", "Posts", new { id = postId });
        }
    }
}
