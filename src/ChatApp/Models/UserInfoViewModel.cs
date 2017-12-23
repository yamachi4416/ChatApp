using System.ComponentModel.DataAnnotations;
using ChatApp.SharedResources;

namespace ChatApp.Models
{
    public class UserInfoViewModel
    {
        [Required(ErrorMessage = SharedResource.Required)]
        [StringLength(30, ErrorMessage = SharedResource.StringLength)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = SharedResource.Required)]
        [StringLength(30, ErrorMessage = SharedResource.StringLength)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
    }
}