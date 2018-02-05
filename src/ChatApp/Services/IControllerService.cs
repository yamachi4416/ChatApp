using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using ChatApp.Data;

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
