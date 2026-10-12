using System.Collections.Generic;

namespace AnonymousSharingPlatform.Models.ViewModels
{
    public class FeedViewModel
    {
        public List<Post> Posts { get; set; } = new List<Post>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public int? SelectedCategoryId { get; set; }
        public string? SelectedCategoryName { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; } // "latest", "popular", "discussed"
        public string? SelectedMood { get; set; }
        public int TotalPostsCount { get; set; }
    }
}
