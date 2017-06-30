using System.ComponentModel.DataAnnotations;

namespace ChatApp.Features.Account.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
