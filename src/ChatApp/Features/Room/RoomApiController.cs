using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Features.Room.Models;
using ChatApp.Controllers;
using ChatApp.Services;
using ChatApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http;
using System.Net;

namespace ChatApp.Features.Room
{
    [Produces("application/json")]
    [Route("api/rooms")]
    [Authorize]
    public class RoomApiController : AppControllerBase
    {
        public RoomApiController(IControllerService service) : base(service)
        {
        }

        protected IQueryable<RoomMemberViewModel> QueryChatMembers()
        {
            var query = (from m in _db.ChatRoomMembers
                         join u in _db.Users on m.UserId equals u.Id
                         orderby m.IsAdmin descending, m.Id
                         select new RoomMemberViewModel
                         {
                             Id = u.Id,
                             RoomId = m.ChatRoomId,
                             Email = u.Email,
                             FirstName = u.FirstName,
                             LastName = u.LastName
                         });
            return query;
        }

        private IQueryable<RoomMessageViewModel> QueryRoomMessages(Guid id)
        {
            var query = (from m in _db.ChatMessages
                         where m.ChatRoomId == id
                         join ux in _db.Users on m.CreatedById equals ux.Id into users
                         from u in users.DefaultIfEmpty()
                         orderby m.Id descending
                         select new RoomMessageViewModel
                         {
                             Id = m.Id,
                             Message = m.Message,
                             UserId = u == null ? "" : u.Id,
                             UserFirstName = u == null ? "" : u.FirstName,
                             UserLastName = u == null ? "" : u.LastName,
                             CreatedDate = m.CreatedDate,
                             UpdatedDate = m.UpdatedDate
                         });

            return query;
        }

        [Route("joins")]
        public async Task<IEnumerable<RoomViewModel>> GetJoinRooms()
        {
            var userId = GetCurrentUserId();
            var query = (from m in _db.ChatRoomMembers
                         where m.UserId == userId
                         join r in _db.ChatRooms on m.ChatRoomId equals r.Id
                         orderby r.Id
                         select new RoomViewModel
                         {
                             Id = r.Id,
                             Name = r.Name,
                             Description = r.Description,
                         });

            return await query.ToListAsync();
        }

        [Route("members/{id}")]
        public async Task<IEnumerable<RoomMemberViewModel>> GetRoomMembers(Guid id)
        {
            var query = QueryChatMembers().Where(m => m.RoomId == id);

            return await query.ToListAsync();
        }

        [Route("messages/{id}/new/{offset?}")]
        public async Task<IEnumerable<RoomMessageViewModel>> GetRoomNewMessages(Guid id, long? offset)
        {
            var query = QueryRoomMessages(id);

            if(offset != null)
            {
                query = query.Where(m => m.Id > offset);
            }

            var messages = await query.Take(20).ToListAsync();
            messages.Reverse();

            return messages;
        }

        [Route("messages/{id}/old/{offset}")]
        public async Task<IEnumerable<RoomMessageViewModel>> GetRoomOldMessages(Guid id, long offset)
        {
            var query = QueryRoomMessages(id).Where(m => m.Id < offset);

            return await query.Take(20).ToListAsync();
        }

        [HttpPost]
        [Route("messages/{id}/create")]
        public async Task<object> MessageCreate([FromRoute]Guid id, [FromBody]PostMessageModel model) {
            if (ModelState.IsValid) {
                var user = await GetCurrentUserAsync();

                var message = CreateModel(new ChatMessage() {
                    ChatRoomId = id,
                    UserId = user.Id,
                    Message = model.Message
                });

                await _db.SaveChangesAsync();

                var viewMessage = new RoomMessageViewModel() {
                    UserFirstName = user.FirstName,
                    UserLastName = user.LastName,
                }.SetChatMessage(message);

                return viewMessage;
            } else {
                Response.StatusCode = 400;
                return ModelState;
            }
        }
    }
}