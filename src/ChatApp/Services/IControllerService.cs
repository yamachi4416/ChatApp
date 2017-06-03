using ChatApp.Data;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Services
{
    public interface IControllerService
    {
        ApplicationDbContext DbContext { get; }

        UserManager<ApplicationUser> UserManager { get; }

        DateTimeOffset DateTimeOffsetNow { get; }
    }
}
