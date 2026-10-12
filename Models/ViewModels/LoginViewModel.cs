using System.ComponentModel.DataAnnotations;

namespace AnonymousSharingPlatform.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please enter admin username.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter admin password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
