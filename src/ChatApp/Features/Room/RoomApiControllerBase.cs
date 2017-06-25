using System;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Controllers;
using ChatApp.Data;
using ChatApp.Features.Room.Models;
using ChatApp.Services;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Features.Room
{
    public class RoomApiControllerBase : AppControllerBase
    {
        public RoomApiControllerBase(IControllerService service) : base(service)
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
                             LastName = u.LastName,
                             IsAdmin = m.IsAdmin
                         });
            return query;
        }

        protected IQueryable<RoomMessageViewModel> QueryRoomMessages(Guid id)
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

        protected IQueryable<RoomMemberViewModel> QueryNotRoomMembers(Guid roomId)
        {
            var query = from u in _db.Users
                        where !(from m in _db.ChatRoomMembers
                                where m.ChatRoomId == roomId
                                select m.UserId).Contains(u.Id)
                        select new RoomMemberViewModel
                        {
                            Id = u.Id,
                            RoomId = roomId,
                            Email = u.Email,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            IsAdmin = false
                        };

            return query;
        }

        protected async Task<ChatRoom> SelectChatRoomById(Guid id)
        {
            var query = from r in _db.ChatRooms
                        where r.Id == id
                        select r;

            return await query.SingleOrDefaultAsync();
        }
    }
}