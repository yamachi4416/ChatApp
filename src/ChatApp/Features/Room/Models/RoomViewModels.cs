using ChatApp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.SharedResources;

namespace ChatApp.Features.Room.Models
{
    public class RoomViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = SharedResource.Required)]
        [StringLength(30, ErrorMessage = SharedResource.StringLength)]
        public string Name { get; set; }

        [StringLength(255, ErrorMessage = SharedResource.StringLength)]
        public string Description { get; set; }

        public bool? IsAdmin { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public DateTimeOffset? UpdatedDate { get; set; }

        public IEnumerable<RoomMemberViewModel> Members { get; set; }

        public IEnumerable<RoomMessageViewModel> Messages { get; set; }
    }

    public class RoomListViewModel
    {
        public ChatRoom Room { get; set; }

        public IEnumerable<RoomMemberViewModel> Members { get; set; }
    }

    public class RoomMemberViewModel
    {
        public string Id { get; set; }

        public Guid? RoomId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public bool IsAdmin { get; set; }

        public Guid? AvatarId { get; set; }
    }

    public class RoomMessageViewModel
    {
        public long? Id { get; set; }

        public string Message { get; set; }

        public string UserId { get; set; }

        public Guid? AvatarId { get; set; }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public DateTimeOffset? UpdatedDate { get; set; }

        public RoomMessageViewModel SetChatMessage(ChatMessage message)
        {
            Id = message.Id;
            Message = message.Message;
            UserId = message.UserId;
            CreatedDate = message.CreatedDate;
            UpdatedDate = message.UpdatedDate;

            return this;
        }
    }

    public class PostMessageModel
    {
        [Required(ErrorMessage = SharedResource.Required)]
        [StringLength(300, ErrorMessage = SharedResource.StringLength)]
        public string Message { get; set; }
    }
}
