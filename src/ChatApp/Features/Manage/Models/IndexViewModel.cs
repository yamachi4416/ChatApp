using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Models;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Features.Manage.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }

        public string Id { get; set; }

        [Display(Name = nameof(Email))]
        public string Email { get; set; }

        public UserInfoViewModel UserInfo { get; set; }

        public IEnumerable<ManageLoginsViewModel> ExternalLogins { get; set; }

        public bool CanRemoveExternalLogin => HasPassword && ExternalLogins.Count() > 1;
    }
}
