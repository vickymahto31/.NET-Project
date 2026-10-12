using System;
using System.ComponentModel.DataAnnotations;

namespace AnonymousSharingPlatform.Models
{
    public class AdminUser
    {
        public int AdminId { get; set; }

        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        public string PasswordHash { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
