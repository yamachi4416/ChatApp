using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Controllers;
using ChatApp.Services;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Features.UserAvatar
{
    public class UserAvatarController : AppControllerBase
    {
        public UserAvatarController(IControllerService service) : base(service)
        {
        }

        private IActionResult DefaultAvatar()
        {
            return Redirect("~/images/avatar.png");
        }

        [HttpGet]
        public async Task<IActionResult> Index(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return DefaultAvatar();
            }

            var att = await (from a in _db.UserAvatars
                             where a.UserId == id
                             select a).SingleOrDefaultAsync();

            if (att == null)
            {
                return DefaultAvatar();
            }

            return File(att.Content, att.ContentType);
        }
    }
}