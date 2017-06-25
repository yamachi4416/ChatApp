using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;
using Microsoft.AspNetCore.Identity;
using ChatApp.Services;
using System.Collections.Generic;
using ChatApp.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ChatApp.Controllers
{
    public partial class AppControllerBase : Controller
    {
        private readonly IControllerService _service;

        private UserManager<ApplicationUser> _userManager { get { return _service.UserManager; } }

        protected ApplicationDbContext _db { get { return _service.DbContext; } }

        protected DateTimeOffset DateTimeOffsetNow { get { return _service.DateTimeOffsetNow; } }

        public AppControllerBase(IControllerService service)
        {
            _service = service;
        }
        
        protected string GetCurrentUserId()
        {
            return _userManager.GetUserId(User);
        }

        protected Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        protected IDictionary<string, IEnumerable<ApiValidationErrorViewModel>> ApiValidateErrorResult()
        {
            Response.StatusCode = 400;
            return CreateApiValidationErrorsMap();
        }

        protected IDictionary<string, IEnumerable<ApiValidationErrorViewModel>>
            CreateApiValidationErrorsMap()
        {
            var errorsMap = new Dictionary<string, IEnumerable<ApiValidationErrorViewModel>>();
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.ValidationState != ModelValidationState.Invalid)
                    continue;

                var errors = new List<ApiValidationErrorViewModel>();
                var jsonKey = char.ToLower(key[0]) + key.Substring(1);

                errorsMap[jsonKey] = errors;

                foreach (var e in state.Errors)
                {
                    var error = new ApiValidationErrorViewModel
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