namespace AnonymousSharingPlatform.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public string BadgeColor { get; set; } = "#2563eb";
        public int PostCount { get; set; }
    }
}
