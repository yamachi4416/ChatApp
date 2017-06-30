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

    [Route("api/rooms/admin/{id}")]
    [Produces("application/json")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public class RoomApiAdminController : RoomApiControllerBase
    {
        public RoomApiAdminController(IControllerService service) : base(service)
        {
        }

        [HttpGet]
        [Route("members/search/{search}")]
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
        [Route("members/remove")]
        public async Task<object> RemoveMember(
            [FromRoute]Guid id,
            [FromBody, Bind("Id")]RoomMemberViewModel member)
        {
            if (ModelState.IsValid)
            {
                var existsQuery =
                    from m in _db.ChatRoomMembers
                    where m.ChatRoomId == id && m.UserId == member.Id
                    select m;
                
                var exists = await existsQuery.SingleOrDefaultAsync();

                if (exists != null && !exists.IsAdmin)
                {
                    _db.ChatRoomMembers.Remove(exists);
                    await _db.SaveChangesAsync();

                    return member;
                }
            }

            return null;
        }

        [HttpPost]
        [Route("members/add")]
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

        [HttpPost]
        [Route("rooms/edit")]
        public async Task<object> EditRoom(
            [FromRoute]Guid id,
            [FromBody, Bind("Id,Name,Description")]RoomViewModel room)
        {
            if (ModelState.IsValid)
            {
                var exists = await SelectChatRoomById(id);

                if (exists == null) 
                {
                    return null;
                }

                UpdateModel(to: exists, from: room, keys: "Name,Description");

                await _db.SaveChangesAsync();

                return MergeModel(to: new RoomViewModel(),
                    from: exists, keys: "Id,Name,Description,UpdatedDate");
            }

            return ApiValidateErrorResult();
        }

        [HttpPost]
        [Route("rooms/remove")]
        public async Task<object> RemoveRoom(Guid id)
        {
            var exists = await SelectChatRoomById(id);

            if (exists != null) 
            {
                _db.ChatRooms.Remove(exists);

                await _db.SaveChangesAsync();

                return new RoomViewModel{ Id = id };
            }

            return null;
        }
    }
}
