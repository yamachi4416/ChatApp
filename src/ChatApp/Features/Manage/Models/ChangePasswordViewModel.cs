using System.ComponentModel.DataAnnotations;
using ChatApp.SharedResources;

namespace ChatApp.Features.Manage.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = SharedResource.Required)]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = SharedResource.Required)]
        [StringLength(100, ErrorMessage = SharedResource.StringLengthMinMax, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare(nameof(NewPassword), ErrorMessage = SharedResource.Compare)]
        public string ConfirmPassword { get; set; }
    }
}
