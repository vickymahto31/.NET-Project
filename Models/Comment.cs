using System;
using System.ComponentModel.DataAnnotations;

namespace AnonymousSharingPlatform.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }

        public string AuthorAlias { get; set; } = "Supportive Peer";

        [Required(ErrorMessage = "Comment cannot be empty.")]
        [StringLength(1500, MinimumLength = 2, ErrorMessage = "Comment must be between 2 and 1500 characters.")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsHidden { get; set; } = false;
    }
}
