using System.Threading.Tasks;
using ChatApp.Controllers;
using ChatApp.Features.Room.Services;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Features.Room
{
    [Authorize]
    [Route("ws/rooms")]
    public class RoomWSController : AppControllerBase
    {
        private readonly IRoomWebSocketService _ws;

        public RoomWSController(
            IControllerService service,
            IRoomWebSocketService ws) : base(service)
        {
            _ws = ws;
        }

        [Route("connect")]
        public async Task<IActionResult> Connect()
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