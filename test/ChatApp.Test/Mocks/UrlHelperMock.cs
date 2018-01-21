using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;

namespace ChatApp.Test.Mocks
{
    public class UrlHelperMock : IUrlHelper
    {

        public ActionContext ActionContext => Mock.Of<ActionContext>();

        public virtual string _action { get; set; }

        public string Action(UrlActionContext actionContext) => _action;

        public virtual string _content { get; set; }

        public string Content(string contentPath) => _content;

        public virtual bool _isLocalUrl { get; set; }

        public bool IsLocalUrl(string url) => _isLocalUrl;

        public virtual string _link { get; set; }

        public string Link(string routeName, object values) => _link;

        public virtual string _routeUrl { get; set; }

        public string RouteUrl(UrlRouteContext routeContext) => _routeUrl;
    }
}