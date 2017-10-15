using System.ComponentModel.DataAnnotations;
using ChatApp.SharedResources;

namespace ChatApp.Features.Account.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = SharedResource.Required)]
        [EmailAddress(ErrorMessage = SharedResource.EmailAddress)]
        [Display(Name = nameof(Email))]
        public string Email { get; set; }
    }
}
