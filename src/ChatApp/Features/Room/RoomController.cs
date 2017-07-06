using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;
using Microsoft.EntityFrameworkCore;
using ChatApp.Controllers;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using ChatApp.Features.Room.Services;

namespace ChatApp.Features.Room
{
    [Authorize]
    public class RoomController : AppControllerBase
    {
        private readonly IRoomWebSocketService _ws;

        public RoomController(IControllerService service,
            IRoomWebSocketService ws) : base(service)
        {
            _ws = ws;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        public async Task<IActionResult> WS()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                var userId = GetCurrentUserId();
                await _ws.AddAsync(userId, HttpContext);
            }
            else
            {
                return new StatusCodeResult(403);
            }

            return new EmptyResult();
        }
    }
}
