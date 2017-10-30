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
        public static string UserAvatar(this IUrlHelper helper)
        {
            return helper.Action("Get", "UserAvatar");
        }

        public static string UserAvatar(this IUrlHelper helper, Guid? id)
        {
            return helper.Action("Get", "UserAvatar", new { id = id });
        }

        public static string ChatRoomAvatar(this IUrlHelper helper, string roomId)
        {
            return helper.Action("Get", "ChatRoomAvatar", new { id = roomId });
        }

        public static string ChatRoomAvatar(this IUrlHelper helper, Guid? roomId)
        {
            return ChatRoomAvatar(helper, roomId?.ToString());
        }
    }
}
