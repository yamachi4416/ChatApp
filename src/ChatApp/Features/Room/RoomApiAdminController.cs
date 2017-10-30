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
using ChatApp.Attributes;
using ChatApp.Features.Room.Services;

namespace ChatApp.Features.Room
{

    [Route("api/rooms/admin/{id}")]
    [ValidateRoomMember(IsAdmin = true)]
    [ValidateAntiForgeryToken]
    public class RoomApiAdminController : RoomApiControllerBase
    {
        public RoomApiAdminController(
            IControllerService service,
            IRoomWebSocketService ws) : base(service, ws)
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

                    await SendWsMessageForUser(
                        roomId: id,
                        userId: exists.UserId,
                        messageType: RoomWsMessageType.DEFECT_ROOM,
                        messageBody: new { }
                    );

                    await SendWsMessageForRoomMembers(
                        roomId: id,
                        excludeUserId: GetCurrentUserId(),
                        messageType: RoomWsMessageType.DELETE_MEMBER,
                        messageBody: new RoomMemberViewModel
                        {
                            Id = exists.UserId
                        }
                    );

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

                var addUser = await addUserQuery.AsNoTracking()
                    .SingleOrDefaultAsync();

                if (addUser != null)
                {
                    var newMember = new ChatRoomMember
                    {
                        ChatRoomId = id,
                        UserId = addUser.Id,
                    };

                    CreateModel(newMember);

                    await _db.SaveChangesAsync();

                    var room = await (
                        from r in _db.ChatRooms.AsNoTracking()
                        where r.Id == id
                        select r).SingleOrDefaultAsync();

                    await SendWsMessageForUser(
                        roomId: id,
                        messageType: RoomWsMessageType.JOIN_ROOM,
                        messageBody: new RoomViewModel
                        {
                            Id = id,
                            Name = room.Name,
                            Description = room.Description,
                            CreatedDate = room.CreatedDate,
                            UpdatedDate = room.UpdatedDate,
                            IsAdmin = false
                        },
                        userId: newMember.UserId);

                    await SendWsMessageForRoomMembers(
                        roomId: id,
                        messageType: RoomWsMessageType.CREATE_MEMBER,
                        messageBody: new RoomMemberViewModel
                        {
                            Id = newMember.UserId,
                            RoomId = id,
                            Email = addUser.Email,
                            FirstName = addUser.FirstName,
                            LastName = addUser.LastName,
                            AvatarId = addUser.AvatarId,
                            IsAdmin = newMember.IsAdmin
                        },
                        excludeUserId: newMember.UserId
                    );

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
                var sendTask = await SendWsMessageForRoomMembersDeferd(
                    roomId: id,
                    messageType: RoomWsMessageType.DELETE_ROOM,
                    messageBody: new { }
                );

                _db.ChatRooms.Remove(exists);
                
                await _db.SaveChangesAsync();

                await sendTask;

                return new RoomViewModel { Id = id };
            }

            return null;
        }
    }
}
