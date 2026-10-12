using System.ComponentModel.DataAnnotations;

namespace AnonymousSharingPlatform.Models.ViewModels
{
    public class ReportPostViewModel
    {
        [Required]
        public int PostId { get; set; }

        public string PostTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a reason for reporting.")]
        public string Reason { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Details cannot exceed 500 characters.")]
        public string? DetailedRemarks { get; set; }
    }
}
