using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.Features.Room.Models;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;

namespace ChatApp.Features.Room {

    [Route("api/rooms/admin")]
    [Produces("application/json")]
    [Authorize]
    public class RoomApiAdminController : RoomApiControllerBase
    {
        public RoomApiAdminController(IControllerService service) : base(service)
        {
        }

        [HttpGet]
        [Route("{id}/members/search/{search}")]
        public async Task<IEnumerable<RoomMemberViewModel>> SearchMembers(
            [FromRoute]Guid id, [FromRoute]string search)
        {
            var users = await QueryNotRoomMembers(id)
                .Where(m => m.Email.Contains(search))
                .Take(100)
                .ToListAsync();

            return users;
        }

        [HttpPost]
        [Route("{id}/members/add")]
        public async Task<object> AddMember(
            [FromRoute]Guid id,
            [FromBody, Bind("Id")]RoomMemberViewModel member)
        {
            if (ModelState.IsValid)
            {
                var addUserQuery =
                    from m in QueryNotRoomMembers(id)
                    where m.Id == member.Id
                    select m;

                var addUser = await addUserQuery.SingleOrDefaultAsync();

                if (addUser != null) {
                    CreateModel(new ChatRoomMember {
                        ChatRoomId = id,
                        UserId = addUser.Id,
                    });

                    await _db.SaveChangesAsync();

                    return addUser;
                }                
            }

            return null;
        }
    }
}