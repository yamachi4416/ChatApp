using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Controllers;
using ChatApp.Services;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Features.ChatRoomAvatar
{
    public class ChatRoomAvatarController : AppControllerBase
    {
        public ChatRoomAvatarController(IControllerService service) : base(service)
        {
        }

        private IActionResult DefaultAvatar()
        {
            return File("~/images/group.png", "image/png");
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid? id)
        {
            if (id == null)
            {
                return DefaultAvatar();
            }

            var att = await _db.ChatRoomAvatars.FirstOrDefaultAsync(a => a.Id == id);

            if (att == null)
            {
                return DefaultAvatar();
            }

            return File(att.Content, att.ContentType);
        }
    }
}
