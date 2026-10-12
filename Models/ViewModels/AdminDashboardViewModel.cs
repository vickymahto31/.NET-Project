using System.Collections.Generic;

namespace AnonymousSharingPlatform.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalPosts { get; set; }
        public int TotalComments { get; set; }
        public int TotalReports { get; set; }
        public int PendingReports { get; set; }
        public int HiddenPosts { get; set; }

        public List<Report> Reports { get; set; } = new List<Report>();
        public List<Post> AllPosts { get; set; } = new List<Post>();
    }
}
