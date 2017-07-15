using System;

namespace ChatApp.Features.Room.Models
{
    public enum RoomWsMessageType
    {
        CREATE_MESSAGE = 10,
        MODIFY_MESSAGE = 11,
        DELETE_MESSAGE = 12,

        CREATE_ROOM = 20,
        MODIFY_ROOM = 22,
        DELETE_ROOM = 23,

        JOIN_ROOM = 24,

        DEFECT_ROOM = 25,

        CREATE_MEMBER = 30,
        DELETE_MEMBER = 33,

    }
    public class RoomWsModel<E>
    {
        public readonly RoomWsMessageType MessageType;

        public readonly E Message;

        public Guid? RoomId { get; set; }

        public RoomWsModel(
            RoomWsMessageType messageType,
            E messageBody,
            Guid? roomId
        )
        {
            MessageType = messageType;
            Message = messageBody;
            RoomId = roomId;
        }
    }
}