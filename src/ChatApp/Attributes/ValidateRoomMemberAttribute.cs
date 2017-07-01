using System;
using System.Linq;
using ChatApp.Controllers;
using ChatApp.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ChatApp.Attributes
{
    public class ValidateRoomMemberAttribute : ActionFilterAttribute, IFilterFactory, IFilterMetadata
    {
        public bool IsAdmin { get; set; }

        public bool IsReusable => false;

        private readonly ApplicationDbContext _context;

        private readonly UserManager<ApplicationUser> _userManager;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var attr = new ValidateRoomMemberAttribute(
                serviceProvider.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext,
                serviceProvider.GetService(
                    typeof(UserManager<ApplicationUser>)) as UserManager<ApplicationUser>);
            
            attr.IsAdmin = IsAdmin;

            return attr;
        }

        public ValidateRoomMemberAttribute()
        {
        }

        public ValidateRoomMemberAttribute(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsRoomMember(ActionExecutingContext filterContext)
        {
            if (!filterContext.ActionArguments.ContainsKey("id"))
            {
                return false;
            }
            
            Guid? roomId = (Guid?)filterContext.ActionArguments["id"];
            if (roomId == null)
            {
                return false;
            }
            
            var userId = _userManager.GetUserId(filterContext.HttpContext.User);

            var query = (
                from m in _context.ChatRoomMembers
                where m.UserId == userId
                join r in _context.ChatRooms on m.ChatRoomId equals r.Id
                where r.Id == roomId
                select m);

            if (IsAdmin)
            {
                query = query.Where(m => m.IsAdmin);
            }

            var member = query.FirstOrDefault();

            if (member == null)
            {
                return false;
            }

            var controller = filterContext.Controller as AppControllerBase;
            controller.ViewBag.IsRoomAdmin = member.IsAdmin;

            return true;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!IsRoomMember(filterContext))
            {
                var ret = new JsonResult(null);
                ret.StatusCode = 403;
                filterContext.Result = ret;
            }
        }
    }

}