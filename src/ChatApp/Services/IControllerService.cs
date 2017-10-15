using ChatApp.Data;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;

namespace ChatApp.Services
{
    public interface IControllerService
    {
        ApplicationDbContext DbContext { get; }

        UserManager<ApplicationUser> UserManager { get; }

        DateTimeOffset DateTimeOffsetNow { get; }

        ILoggerFactory LoggerFactory { get; }

        IStringLocalizerFactory LocalizeFactory { get; }
    }
}
