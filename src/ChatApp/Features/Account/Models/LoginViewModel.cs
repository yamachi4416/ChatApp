using System.ComponentModel.DataAnnotations;
using ChatApp.SharedResources;

namespace ChatApp.Features.Account.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = SharedResource.Required)]
        [EmailAddress(ErrorMessage = SharedResource.EmailAddress)]
        [Display(Name = nameof(Email))]
        public string Email { get; set; }

        [Required(ErrorMessage = SharedResource.Required)]
        [DataType(DataType.Password)]
        [Display(Name = nameof(Password))]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
