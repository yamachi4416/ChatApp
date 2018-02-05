using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Controllers;
using ChatApp.Features.Account.Models;

namespace ChatApp.Features.Account
{
    public partial class AccountController : AppControllerBase
    {

        [HttpGet]
        public async Task<ActionResult> Edit()
        {
            var user = await GetCurrentUserAsync();
            var model = new EditUserInfoViewModel
            {
                Id = user.Id,
                LastName = user.LastName,
                FirstName = user.FirstName
            };

            return View(model);
        }

    }
}
