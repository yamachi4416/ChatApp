using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Controllers;
using ChatApp.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using ChatApp.Features.UserAvatar.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ChatApp.Features.UserAvatar
{
    [Authorize]
    public class UserAvatarController : AppControllerBase
    {
        public UserAvatarController(IControllerService service) : base(service)
        {
        }

        private IActionResult DefaultAvatar()
        {
            return RedirectToLocal("~/images/avatar.png");
        }

        [HttpGet]
        public async Task<IActionResult> Get(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                id = GetCurrentUserId();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(UploadAvatarModel model)
        {
            if (!ModelState.IsValid)
            {
                return JsonValidateErrorResult();
            }

            var avatar = new Data.UserAvatar
            {
                UserId = GetCurrentUserId(),
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

            var exists = await _db.UserAvatars.SingleOrDefaultAsync(a => a.UserId == avatar.UserId);
            if (exists != null)
            {
                exists.Content = avatar.Content;
                exists.ContentType = avatar.ContentType;
            }
            else
            {
                _db.UserAvatars.Add(avatar);
            }

            await _db.SaveChangesAsync();

            return Json("OK");
        }
    }
}
