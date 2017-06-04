using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Features.Room.Models;
using ChatApp.Controllers;
using ChatApp.Services;

namespace ChatApp.Features.Room
{
    [Produces("application/json")]
    [Route("api/Room")]
    public class RoomApiController : AppControllerBase
    {
        public RoomApiController(IControllerService service) : base(service)
        {
        }

        public IEnumerable<RoomListViewModel> GetList()
        {
            var query = (from r in _db.ChatRooms
                         select r);

            return null;
        }
    }
}