using System;
using System.Threading.Tasks;
using ChatApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ChatApp.Services.RoomwebSocket
{
    public class RoomWebSocketServer
    {
        private readonly RequestDelegate _next;

        private readonly string _path;

        private readonly IRoomWebSocketService _ws;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly ILogger _logger;

        public RoomWebSocketServer(
            RequestDelegate next,
            string path,
            IRoomWebSocketService ws,
            UserManager<ApplicationUser> userManager,
            ILoggerFactory _loggerFactory)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("path is required.");
            }

            _next = next;
            _path = path;
            _ws = ws;
            _userManager = userManager;
            _logger = _loggerFactory.CreateLogger(GetType());
        }

        public async Task Invoke(HttpContext context)
        {
            if (!IsAvailable(context))
            {
                await _next(context);
                return;
            }

            var userId = _userManager.GetUserId(context.User);
            _logger.LogInformation("websocket connect user [{0}]", userId);
            await _ws.AddAsync(userId, context);
            _logger.LogInformation("websocket close user [{0}]", userId);
        }

        private bool IsAvailable(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(_path)
                && context.WebSockets.IsWebSocketRequest
                && context.User?.Identity?.IsAuthenticated == true;
        }
    }
}