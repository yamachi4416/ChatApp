using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Features.Manage.Models
{
    public class ManageLoginsViewModel
    {
        public string ProviderName { get; set; }
        
        public UserLoginInfo CurrentLogin { get; set; }

        public AuthenticationDescription OtherLogin { get; set; }
    }
}
