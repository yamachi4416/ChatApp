using System.ComponentModel.DataAnnotations;
using ChatApp.SharedResources;

namespace ChatApp.Features.Account.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = SharedResource.Required)]
        [EmailAddress(ErrorMessage = SharedResource.EmailAddress)]
        [Display(Name = nameof(Email))]
        public string Email { get; set; }

        [Required(ErrorMessage = SharedResource.Required)]
        [StringLength(100, ErrorMessage = SharedResource.StringLengthMinMax, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = nameof(Password))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = SharedResource.Compare)]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }
}
