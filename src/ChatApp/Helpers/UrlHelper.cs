using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Helpers
{
    public static class UrlHelperExt
    {
        public static string UserAvatar(this UrlHelper helper, string userId)
        {
            return helper.Action("Index", "UserAvatar", new { id = userId });
        }

        public static string ChatRoomAvatar(this UrlHelper helper, string roomId)
        {
            return helper.Action("Index", "ChatRoomAvatar", new { id = roomId });
        }

        public static string ChatRoomAvatar(this UrlHelper helper, Guid? roomId)
        {
            return ChatRoomAvatar(helper, roomId?.ToString());
        }
    }
}
