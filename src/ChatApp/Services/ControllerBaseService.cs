using ChatApp.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public class ControllerBaseService : IControllerService
    {
        private ApplicationDbContext _db;

        private UserManager<ApplicationUser> _userManager;

        private IDateTimeService _dateTimeSerice;

        public ApplicationDbContext DbContext { get { return _db; } }

        public UserManager<ApplicationUser> UserManager { get { return _userManager; } }

        public DateTimeOffset DateTimeOffsetNow { get { return _dateTimeSerice.Now; } }

        public ControllerBaseService(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            IDateTimeService dateTimeService)
        {
            _db = db;
            _userManager = userManager;
            _dateTimeSerice = dateTimeService;
        }
    }
}
