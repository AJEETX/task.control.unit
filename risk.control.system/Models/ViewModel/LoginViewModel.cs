using System.ComponentModel.DataAnnotations;

namespace risk.control.system.Models.ViewModel
{
    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }

        public bool Mobile { get; set; }

        public string? Error { get; set; }
    }
}