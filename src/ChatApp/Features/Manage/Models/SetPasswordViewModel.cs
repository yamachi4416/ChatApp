using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.SharedResources;

namespace ChatApp.Features.Manage.Models
{
    public class SetPasswordViewModel
    {
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
