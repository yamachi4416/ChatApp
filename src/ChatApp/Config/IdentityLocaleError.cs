using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using ChatApp.SharedResources;

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

        public override IdentityError PasswordRequiresNonAlphanumeric()
            => GetError(nameof(PasswordRequiresNonAlphanumeric), "Passwords must have at least one non alphanumeric character.");

        public override IdentityError PasswordRequiresDigit()
            => GetError(nameof(PasswordRequiresDigit), "Passwords must have at least one digit ('0'-'9').");

        public override IdentityError PasswordRequiresUpper()
            => GetError(nameof(PasswordRequiresUpper), "Passwords must have at least one uppercase ('A'-'Z').");

        public override IdentityError PasswordMismatch()
            => GetError(nameof(PasswordMismatch), "Incorrect password.");

        public override IdentityError PasswordRequiresLower()
            => GetError(nameof(PasswordRequiresLower), "Passwords must have at least one lowercase ('a'-'z').");

        public override IdentityError PasswordRequiresUniqueChars(int leastCount)
            => GetError(nameof(PasswordRequiresUniqueChars), "Passwords must use at least {0} different characters.", leastCount);

        public override IdentityError PasswordTooShort(int leastCount)
            => GetError(nameof(PasswordTooShort), "Passwords must be at least {0} characters.", leastCount);

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
