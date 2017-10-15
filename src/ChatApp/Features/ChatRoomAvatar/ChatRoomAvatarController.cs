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
            return RedirectToLocal("~/images/group.png");
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            Guid chatRoomId;
            if (!Guid.TryParse(id, out chatRoomId))
            {
                return DefaultAvatar();
            }

            var att = await (from a in _db.ChatRoomAvatars
                             where a.ChatRoomId == chatRoomId
                             select a).SingleOrDefaultAsync();

            if (att == null)
            {
                return DefaultAvatar();
            }

            return File(att.Content, att.ContentType);
        }
    }
}
