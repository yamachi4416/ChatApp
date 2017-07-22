using System.Threading.Tasks;
using ChatApp.Controllers;
using ChatApp.Features.Account.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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