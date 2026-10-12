using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AnonymousSharingPlatform.Models.ViewModels
{
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "Please give your thought a title.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please write what is on your mind.")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Content must be between 10 and 5000 characters.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please choose a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid category.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Please select your current emotion/mood.")]
        public string MoodTag { get; set; } = "Venting";

        [StringLength(50, ErrorMessage = "Alias cannot exceed 50 characters.")]
        public string AuthorAlias { get; set; } = "Anonymous Student";

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must accept the community guidelines to post.")]
        public bool AcceptCommunityRules { get; set; }

        public List<SelectListItem> CategoryOptions { get; set; } = new List<SelectListItem>();
    }
}
