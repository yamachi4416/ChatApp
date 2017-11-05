using System;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services.RoomwebSocket
{
    public interface IRoomWSSender
    {
        Task SendWsMessageForUser<E>(Guid roomId, RoomWsMessageType messageType, E messageBody, string userId);

        Task<Task> SendWsMessageForRoomMembersDeferd<E>(Guid roomId, RoomWsMessageType messageType, E messageBody, string excludeUserId = null);

        Task SendWsMessageForRoomMembers<E>(Guid roomId, RoomWsMessageType messageType, E messageBody, string excludeUserId = null);
    }

    public class RoomWSSender : IRoomWSSender
    {
        protected readonly ApplicationDbContext _db;

        protected readonly IRoomWebSocketService _ws;
        
        public RoomWSSender(ApplicationDbContext db, IRoomWebSocketService ws)
        {
            this._db = db;
            this._ws = ws;
        }

        public async Task SendWsMessageForUser<E>(
            Guid roomId,
            RoomWsMessageType messageType,
            E messageBody,
            string userId)
        {
            var message = new RoomWsModel<E>(
                messageType: messageType,
                messageBody: messageBody,
                roomId: roomId
            );
            
            await _ws.SendAsync(message, userId);
        }

        public async Task<Task> SendWsMessageForRoomMembersDeferd<E>(
            Guid roomId,
            RoomWsMessageType messageType,
            E messageBody,
            string excludeUserId = null)
        {
            var query =
                from m in _db.ChatRoomMembers
                where m.ChatRoomId == roomId
                select m.UserId;

            if (!string.IsNullOrEmpty(excludeUserId))
            {
                query = query.Where(m => m != excludeUserId);
            }

            var message = new RoomWsModel<E>(
                messageType: messageType,
                messageBody: messageBody,
                roomId: roomId
            );

            return _ws.SendAsync(message, await query.AsNoTracking().ToListAsync());
        }

        public async Task SendWsMessageForRoomMembers<E>(
            Guid roomId,
            RoomWsMessageType messageType,
            E messageBody,
            string excludeUserId = null)
        {
            var task = SendWsMessageForRoomMembersDeferd(
                roomId: roomId,
                messageType: messageType,
                messageBody: messageBody,
                excludeUserId: excludeUserId
            );

            await task;
        }
    }
}