using System.Collections.Generic;

namespace AnonymousSharingPlatform.Models.ViewModels
{
    public class PostDetailsViewModel
    {
        public Post Post { get; set; } = new Post();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public Comment NewComment { get; set; } = new Comment();
        public Report NewReport { get; set; } = new Report();
    }
}
