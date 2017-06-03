using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;
using Microsoft.EntityFrameworkCore;
using ChatApp.Controllers;
using ChatApp.Services;

namespace ChatApp.Features.Room
{
    public class RoomController : AppControllerBase
    {
        public RoomController(IControllerService service) : base(service)
        {

        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create([Bind(include: "Name,Description")]ChatRoom room)
        {
            if (ModelState.IsValid)
            {
                CreateModel(room);
                CreateModel(new ChatRoomMember
                {
                    UserId = GetCurrentUserId(),
                    ChatRoom = room,
                    IsAdmin = true
                });

                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Edit), new { id = room.Id });
            }

            return View(room);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            var query = _db.ChatRooms.Where(r => r.Id == id);
            var room = await query.SingleOrDefaultAsync();

            if (room == null)
            {
                return NotFound();
            }

            return View(room);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind(include: "Id,Name,Description")]ChatRoom room)
        {
            if (ModelState.IsValid)
            {
                var chatRoom = await _db.ChatRooms
                    .Where(r => r.Id == room.Id)
                    .SingleOrDefaultAsync();

                UpdateModel(from: room, to: chatRoom);

                await _db.SaveChangesAsync();

                room = chatRoom;
            }

            return View(room);
        }
    }
}