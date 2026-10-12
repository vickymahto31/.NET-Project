using System;
using System.ComponentModel.DataAnnotations;

namespace AnonymousSharingPlatform.Models
{
    public class Report
    {
        public int ReportId { get; set; }

        public int PostId { get; set; }
        public string? PostTitle { get; set; }
        public string? PostContentSnippet { get; set; }

        [Required(ErrorMessage = "Please select a reason for reporting.")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? DetailedRemarks { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Reviewed, Dismissed, ActionTaken
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
