using ChatApp.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Features.Room.Models
{
    public class RoomViewModel
    {
        public Guid? Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

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
    }

    public class RoomMessageViewModel
    {
        public long? Id { get; set; }

        public string Message { get; set; }

        public string UserId { get; set; }

        public string UserFirstName { get; set; }

        public string UserLastName { get; set; }

        public DateTimeOffset? CreatedDate { get; set; }

        public DateTimeOffset? UpdatedDate { get; set; }
    }

    public class PostMessageModel
    {
        [Required]
        [MaxLength(300)]
        public string Message { get; set; }
    }
}
