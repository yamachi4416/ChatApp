using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;
using Microsoft.AspNetCore.Identity;
using ChatApp.Services;
using System.Collections.Generic;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using ChatApp.SharedResources;

namespace ChatApp.Controllers
{
    public partial class AppControllerBase : Controller
    {
        private readonly IControllerService _service;

        protected UserManager<ApplicationUser> _userManager { get { return _service.UserManager; } }

        protected ApplicationDbContext _db { get { return _service.DbContext; } }

        protected DateTimeOffset DateTimeOffsetNow { get { return _service.DateTimeOffsetNow; } }

        protected ILogger _logger { get; }

        protected IStringLocalizer _localizer { get; }

        public AppControllerBase(IControllerService service)
        {
            _service = service;
            _logger = service.LoggerFactory.CreateLogger(GetType());
            _localizer = service.LocalizeFactory.Create(typeof(SharedResource));
        }

        protected string GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        protected Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        protected IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction(nameof(Features.Home.HomeController.Index), "Home");
            }
        }

        protected IDictionary<string, IEnumerable<ValidationErrorViewModel>> ApiValidateErrorResult()
        {
            Response.StatusCode = 400;
            return CreateApiValidationErrorsMap(camelCase: true);
        }

        protected IActionResult JsonValidateErrorResult(bool camelCase = false)
        {
            Response.StatusCode = 400;
            return Json(CreateApiValidationErrorsMap(camelCase: camelCase));
        }

        protected IDictionary<string, IEnumerable<ValidationErrorViewModel>> CreateApiValidationErrorsMap(bool camelCase = true)
        {
            var errorsMap = new Dictionary<string, IEnumerable<ValidationErrorViewModel>>();
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.ValidationState != ModelValidationState.Invalid)
                    continue;

                var errors = new List<ValidationErrorViewModel>();

                var jsonKey = key;
                if (camelCase)
                {
                    jsonKey = char.ToLower(key[0]) + (key.Length > 1 ? key.Substring(1) : string.Empty);
                }

                errorsMap[jsonKey] = errors;

                foreach (var e in state.Errors)
                {
                    var error = new ValidationErrorViewModel
                    {
                        ErrorMessage = e.ErrorMessage,
                        Key = jsonKey
                    };

                    errors.Add(error);
                }
            }
            return errorsMap;
        }
    }
}
