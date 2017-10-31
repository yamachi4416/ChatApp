using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Controllers;
using ChatApp.Services;
using Microsoft.EntityFrameworkCore;
using ChatApp.Features.ChatRoomAvatar.Models;
using ChatApp.Attributes;
using ChatApp.Data;

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

        [HttpPost]
        [Route("api/rooms/admin/{id}/avatars/upload")]
        [AutoValidateAntiforgeryToken]
        [ValidateRoomMember(IsAdmin = true)]
        public async Task<IActionResult> Upload([FromRoute]Guid id, UploadAvatarModel model)
        {
            if (!ModelState.IsValid)
            {
                return JsonValidateErrorResult();
            }

            var avatar = new Data.ChatRoomAvatar
            {
                ChatRoomId = id,
                Content = new byte[model.ImageFile.Length],
                ContentType = model.ImageFile.ContentType
            };

            if (!TryValidateModel(avatar))
            {
                return JsonValidateErrorResult();
            }

            using (var upFile = model.ImageFile.OpenReadStream())
            {
                await upFile.ReadAsync(avatar.Content, 0, avatar.Content.Length);
            }

            var updated = await UpdateAvatar(avatar);

            return Json(updated.ChatRoomAvatarId);
        }

        private async Task<ChatRoom> UpdateAvatar(Data.ChatRoomAvatar avatar)
        {
            using (var tx = await _db.Database.BeginTransactionAsync())
            {
                var id = avatar.ChatRoomId;
                var room = await _db.ChatRooms
                    .Where(r => r.Id == id).SingleOrDefaultAsync();

                if (room != null)
                {
                    var exists = await _db.ChatRoomAvatars
                        .Where(a => a.ChatRoomId == id).ToListAsync();

                    if (exists.Count != 0)
                    {
                        _db.ChatRoomAvatars.RemoveRange(exists);
                    }

                    await _db.ChatRoomAvatars.AddAsync(avatar);
                    await _db.SaveChangesAsync();

                    room.ChatRoomAvatarId = avatar.Id;
                    await _db.SaveChangesAsync();

                    tx.Commit();
                }

                return room;
            }
        }
    }
}
