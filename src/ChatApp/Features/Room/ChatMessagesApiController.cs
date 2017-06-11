using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using ChatApp.Features.Room.Models;
using ChatApp.Controllers;
using ChatApp.Services;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;

namespace ChatApp.Features.Room
{
    [Route("api/RoomMessages/{id}/{messageId?}")]
    public class ChatMessagesApiController : AppControllerBase
    {
        private readonly int _takeCount = 30;

        public ChatMessagesApiController(IControllerService service) : base(service)
        {
        }

        [HttpGet]
        public async Task<IEnumerable<RoomMessageViewModel>> Get([FromRoute] Guid id, long? offset)
        {
            offset = offset ?? -1;

            var query = (from m in _db.ChatMessages
                         where m.ChatRoomId == id && m.Id > offset
                         join tu in _db.Users on m.CreatedById equals tu.Id into users
                         from u in users.DefaultIfEmpty()
                         orderby m.Id descending
                         select new RoomMessageViewModel
                         {
                             Id = m.Id,
                             Message = m.Message,
                             UserId = u == null ? null : u.Id,
                             UserFirstName = u == null ? null : u.FirstName,
                             UserLastName = u == null ? null : u.LastName,
                             UpdatedDate = m.UpdatedDate,
                             CreatedDate = m.CreatedDate
                         }).Take(_takeCount);

            var messages = await query.ToListAsync();
            messages.Reverse();

            return messages;
        }

        [HttpPost]
        public async Task<string> Post([FromRoute] Guid id, [FromBody, Bind(include: "Message")]PostMessageModel message)
        {
            if (ModelState.IsValid)
            {
                CreateModel(new ChatMessage {
                    ChatRoomId = id,
                    Message = message.Message
                });

                await _db.SaveChangesAsync();
            }

            return "OK";
        }
    }
}
