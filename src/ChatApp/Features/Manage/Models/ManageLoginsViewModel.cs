using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace ChatApp.Features.Manage.Models
{
    public class ManageLoginsViewModel
    {
        public string LoginProvider => Provider.Name;

        public string DisplayName => Provider.DisplayName;

        public string ProviderKey => LoginInfo?.ProviderKey;

        public bool IsLoggedIn => LoginInfo != null;

        public UserLoginInfo LoginInfo { get; set; }

        public AuthenticationScheme Provider { get; set; }

        public ManageLoginsViewModel(AuthenticationScheme provider, IEnumerable<UserLoginInfo> loginInfos)
        {
            Provider = provider;
            LoginInfo = loginInfos.SingleOrDefault(logininfo => provider.Name == logininfo.LoginProvider);
        }
    }
}
