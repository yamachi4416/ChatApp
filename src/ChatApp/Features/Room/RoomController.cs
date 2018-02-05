using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ChatApp.Controllers;
using ChatApp.Services;

namespace ChatApp.Features.Room
{
    [Authorize]
    public class RoomController : AppControllerBase
    {
        public RoomController(IControllerService service) : base(service)
        {
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
