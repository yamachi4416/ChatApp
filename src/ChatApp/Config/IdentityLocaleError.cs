using ChatApp.SharedResources;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace ChatApp.IdentityLocaleError
{
    public class IdentityLocaleErrorDescriber : IdentityErrorDescriber
    {
        private readonly IStringLocalizer _localizer;

        public IdentityLocaleErrorDescriber(IStringLocalizerFactory localizerFactory) : base()
        {
            _localizer = localizerFactory.Create(typeof(SharedResource));
        }

        public override IdentityError DuplicateEmail(string email)
            => GetError(nameof(DuplicateEmail), "Email '{0}' is already taken.", email);

        public override IdentityError DuplicateUserName(string userName)
            => GetError(nameof(DuplicateUserName), "User name '{0}' is already taken.", userName);

        private IdentityError GetError(string code, string message, params object[] arguments)
        {
            return new IdentityError
            {
                Code = code,
                Description = _localizer[message, arguments]
            };
        }
    }
}