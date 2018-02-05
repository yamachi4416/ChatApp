using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ChatApp.Services;
using ChatApp.Controllers;
using ChatApp.Features.UserAvatar.Models;

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
            return File("~/images/avatar.png", "image/png");
        }

        [HttpGet]
        [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 120 * 60)]
        public async Task<IActionResult> Get(Guid? id)
        {
            if (id == null)
            {
                return DefaultAvatar();
            }
            
            var att = await _db.UserAvatars.Select(a => a)
                .Where(a => a.Id == id)
                .SingleOrDefaultAsync();

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

            var user = await GetCurrentUserAsync();
            var avatar = new Data.UserAvatar
            {
                UserId = user.Id,
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

            var exists = await (from a in _db.UserAvatars
                                where a.UserId == avatar.UserId
                                select new Data.UserAvatar { Id = a.Id }).ToListAsync();

            if (exists.Count != 0)
            {
                _db.UserAvatars.RemoveRange(exists);
            }

            _db.UserAvatars.Add(avatar);

            await _db.SaveChangesAsync();

            user.UserAvatarId = avatar.Id;

            await _userManager.UpdateAsync(user);

            return Json(new Data.UserAvatar { Id = avatar.Id });
        }
    }
}
