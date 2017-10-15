using System.ComponentModel.DataAnnotations;
using ChatApp.SharedResources;

namespace ChatApp.Features.Account.Models
{
    public class VerifyCodeViewModel
    {
        [Required(ErrorMessage = SharedResource.Required)]
        public string Provider { get; set; }

        [Required(ErrorMessage = SharedResource.Required)]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
