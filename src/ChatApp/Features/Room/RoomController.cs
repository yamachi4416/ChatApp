using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;
using Microsoft.EntityFrameworkCore;
using ChatApp.Controllers;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;

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
