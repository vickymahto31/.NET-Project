using System;
using System.ComponentModel.DataAnnotations;

namespace AnonymousSharingPlatform.Models
{
    public class Post
    {
        public int PostId { get; set; }

        [Required(ErrorMessage = "Please provide a title for your post.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please describe what is on your mind.")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 5000 characters.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please choose a valid category.")]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }
        public string? CategoryBadgeColor { get; set; }

        [Required(ErrorMessage = "Please choose a mood or emotion tag.")]
        public string MoodTag { get; set; } = "Venting";

        public string AuthorAlias { get; set; } = "Anonymous Student";
        public int SupportCount { get; set; } = 0;
        public int ReportCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public bool IsHidden { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
