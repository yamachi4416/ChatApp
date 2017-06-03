using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;
using Microsoft.AspNetCore.Identity;
using ChatApp.Services;

namespace ChatApp.Controllers
{
    public partial class AppControllerBase : Controller
    {
        private readonly IControllerService _service;

        private UserManager<ApplicationUser> _userManager { get { return _service.UserManager; } }

        protected ApplicationDbContext _db { get { return _service.DbContext; } }

        protected DateTimeOffset DateTimeOffsetNow { get { return _service.DateTimeOffsetNow; } }

        public AppControllerBase(IControllerService service)
        {
            _service = service;
        }
        
        protected string GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        protected Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

    }
}