using ChatApp.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace ChatApp.Services
{
    public class ControllerBaseService : IControllerService
    {
        private readonly ApplicationDbContext _db;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IDateTimeService _dateTimeSerice;

        private readonly ILoggerFactory _loggerFactory;

        private readonly IStringLocalizerFactory _localizeFactory;

        public ApplicationDbContext DbContext => _db;

        public UserManager<ApplicationUser> UserManager => _userManager;

        public DateTimeOffset DateTimeOffsetNow => _dateTimeSerice.Now;

        public ILoggerFactory LoggerFactory => _loggerFactory;

        public IStringLocalizerFactory LocalizeFactory => _localizeFactory;

        public ControllerBaseService() {}

        public ControllerBaseService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IDateTimeService dateTimeService,
            IStringLocalizerFactory stringLocalizerFactory,
            ILoggerFactory loggerFactory)
        {
            _db = db;
            _userManager = userManager;
            _dateTimeSerice = dateTimeService;
            _localizeFactory = stringLocalizerFactory;
            _loggerFactory = loggerFactory;
        }
    }
}
