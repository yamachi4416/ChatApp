using System.ComponentModel.DataAnnotations;

namespace ChatApp.Features.Account.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
