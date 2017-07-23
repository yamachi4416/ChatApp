using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ChatApp.Services;
using ChatApp.Data;
using ChatApp.Features.Manage.Models;
using ChatApp.Models;
using Microsoft.AspNetCore.Authentication;
using ChatApp.Controllers;

namespace ChatApp.Features.Manage
{
    [Authorize]
    public class ManageController : AppControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger _logger;

        public ManageController(
            IControllerService service,
            IEmailSender emailSender,
            SignInManager<ApplicationUser> signInManager,
            ILoggerFactory loggerFactory) : base(service)
        {
            _signInManager = signInManager;
            _emailSender = emailSender;
            _logger = loggerFactory.CreateLogger(nameof(GetType));
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();

            if (user == null)
            {
                return View("Error");
            }

            var loginInfos = await _userManager.GetLoginsAsync(user);
            var externalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
                .Select(provider => new ManageLoginsViewModel(provider: provider, loginInfos: loginInfos));

            var model = new IndexViewModel
            {
                Id = user.Id,
                Email = user.Email,
                HasPassword = await _userManager.HasPasswordAsync(user),
                ExternalLogins = externalLogins,
                UserInfo = new UserInfoViewModel
                {
                    LastName = user.LastName,
                    FirstName = user.FirstName
                }
            };

            SetViewBagFromTempMessage();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveLogin(RemoveLoginViewModel account)
        {
            ManageMessageId? message = ManageMessageId.Error;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var hasPassword = await _userManager.HasPasswordAsync(user);
                var loginInfos = await _userManager.GetLoginsAsync(user);

                if (!hasPassword && loginInfos.Count == 1)
                {
                    return RedirectToIndex(message);
                }
                
                var result = await _userManager.RemoveLoginAsync(user, account.LoginProvider, account.ProviderKey);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    message = ManageMessageId.RemoveLoginSuccess;
                }
            }

            return RedirectToIndex(message);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "User changed their password successfully.");

                    return RedirectToIndex(ManageMessageId.ChangePasswordSuccess);
                }
                AddErrors(result);
                return View(model);
            }

            return RedirectToIndex(ManageMessageId.Error);
        }

        //
        // GET: /Manage/SetPassword
        [HttpGet]
        public IActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.AddPasswordAsync(user, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToIndex(ManageMessageId.SetPasswordSuccess);
                }
                AddErrors(result);
                return View(model);
            }

            return RedirectToIndex(ManageMessageId.Error);
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LinkLogin(string provider)
        {
            var redirectUrl = Url.Action(nameof(LinkLoginCallback), "Manage");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
            return Challenge(properties, provider);
        }

        //
        // GET: /Manage/LinkLoginCallback
        [HttpGet]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var info = await _signInManager.GetExternalLoginInfoAsync(await _userManager.GetUserIdAsync(user));
            if (info == null)
            {
                return RedirectToIndex(ManageMessageId.Error);
            }
            var result = await _userManager.AddLoginAsync(user, info);
            var message = result.Succeeded ? ManageMessageId.AddLoginSuccess : ManageMessageId.Error;

            return RedirectToIndex(message);
        }


        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private ActionResult RedirectToIndex(ManageMessageId? messageId)
        {
            SetTempMessage(messageId);
            return RedirectToAction(nameof(Index));
        }

        private void SetTempMessage(ManageMessageId? messageId)
        {
            var message =
                messageId == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : messageId == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : messageId == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : messageId == ManageMessageId.AddLoginSuccess ? "The external login was added."
                : messageId == ManageMessageId.Error ? "An error has occurred."
                : "";

            TempData["StatusMessage"] = message;
        }

        private void SetViewBagFromTempMessage()
        {
            var message = TempData["StatusMessage"]?.ToString();
            ViewData["StatusMessage"] = message;
        }

        public enum ManageMessageId
        {
            AddLoginSuccess,
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            Error
        }
    }
}
