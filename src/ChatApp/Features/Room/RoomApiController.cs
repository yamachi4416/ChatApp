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

namespace ChatApp.Features.Room
{
    [Produces("application/json")]
    [Route("api/rooms")]
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

            return await query.Take(30).ToListAsync();
        }

        [Route("messages/{id}/old/{offset}")]
        public async Task<IEnumerable<RoomMessageViewModel>> GetRoomOldMessages(Guid id, long offset)
        {
            var query = QueryRoomMessages(id).Where(m => m.Id < offset);

            return await query.Take(30).ToListAsync();
        }
    }
}