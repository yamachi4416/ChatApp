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
    public class RoomApiController : RoomApiControllerBase
    {
        private readonly int _takeCount = 30;

        public RoomApiController(IControllerService service) : base(service)
        {
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
                             IsAdmin = m.IsAdmin,
                             CreatedDate = r.CreatedDate,
                             UpdatedDate = r.UpdatedDate
                         });

            return await query.ToListAsync();
        }

        [HttpPost]
        [Route("rooms/create")]
        public async Task<object> CreateRoom(
            [FromBody, Bind(include: "Name,Description")]RoomViewModel room)
        {
            if (ModelState.IsValid)
            {
                var newRoom = CreateModel(new ChatRoom(), room, "Name,Description");
                CreateModel(new ChatRoomMember
                {
                    UserId = GetCurrentUserId(),
                    ChatRoom = newRoom,
                    IsAdmin = true
                });

                await _db.SaveChangesAsync();

                return new RoomViewModel
                {
                    Id = newRoom.Id,
                    Name = newRoom.Name,
                    Description = newRoom.Description,
                    IsAdmin = true
                };
            }

            return ApiValidateErrorResult();
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

            var messages = await query.Take(_takeCount).ToListAsync();
            messages.Reverse();

            return messages;
        }

        [Route("messages/{id}/old/{offset}")]
        public async Task<IEnumerable<RoomMessageViewModel>> GetRoomOldMessages(Guid id, long offset)
        {
            var query = QueryRoomMessages(id).Where(m => m.Id < offset);

            return await query.Take(_takeCount).ToListAsync();
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
            }

            return ApiValidateErrorResult();
        }
    }
}