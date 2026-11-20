using System.ComponentModel.DataAnnotations;

namespace CompanyName.Web.Models.Login
{
    public sealed class LoginDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
