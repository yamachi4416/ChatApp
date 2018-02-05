using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ChatApp.Models;

namespace ChatApp.Features.Manage.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }

        public string Id { get; set; }

        public Guid? AvatarId { get; set; }

        [Display(Name = nameof(Email))]
        public string Email { get; set; }

        public UserInfoViewModel UserInfo { get; set; }

        public IEnumerable<ManageLoginsViewModel> ExternalLogins { get; set; }

        public bool CanRemoveExternalLogin => HasPassword || ExternalLogins.Count() > 1;
    }
}
