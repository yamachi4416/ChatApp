using ChatApp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Features.Room.Models
{
    public class RoomListViewModel
    {
        public ChatRoom Room { get; set; }

        public IEnumerable<RoomMemberViewModel> Members { get; set; }
    }

    public class RoomMemberViewModel
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
    }
}
