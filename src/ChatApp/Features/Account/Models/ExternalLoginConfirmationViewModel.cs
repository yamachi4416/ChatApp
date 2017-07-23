using System.ComponentModel.DataAnnotations;
using ChatApp.Models;

namespace ChatApp.Features.Account.Models
{
    public class ExternalLoginConfirmationViewModel : UserInfoViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
