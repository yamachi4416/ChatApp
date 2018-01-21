using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ChatApp.Data;

namespace ChatApp.Test.Mocks
{
    public class SignInManagerMock : SignInManager<ApplicationUser>
    {
        public SignInManagerMock(UserManager<ApplicationUser> userManager) : base(
            userManager,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>().Object, null, null, null)
        {

        }
    }
}