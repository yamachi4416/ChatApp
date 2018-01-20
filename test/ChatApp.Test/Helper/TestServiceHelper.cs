using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatApp.Data;
using ChatApp.Services;
using ChatApp.Services.RoomwebSocket;
using ChatApp.Test.Mock;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AngleSharp.Parser.Html;
using AngleSharp.Dom.Html;

namespace ChatApp.Test.Helper
{
    public class TestServiceHelper
    {
        public readonly string DefaultPassword = "P@ssw0rd";

        public readonly TestServer Server;

        public IServiceProvider ServiceProvider => Server.Host.Services;

        public IControllerService ControllerService => ServiceProvider.GetService<IControllerService>();

        public ApplicationDbContext DbContext => ControllerService.DbContext;

        public UserManager<ApplicationUser> UserManager => ControllerService.UserManager;

        public IRoomWSSender WsSender => ServiceProvider.GetService<IRoomWSSender>();

        public EmailSenderMock MailSender => ServiceProvider.GetService<IEmailSender>() as EmailSenderMock; 

        public TestServiceHelper()
        {
            var serverBuilder = new TestServerBuilder();
            Server = serverBuilder.CreateTestServer();
        }

        public TestServiceHelper(TestServer testServer)
        {
            Server = testServer;
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password = null)
        {
            var result = await ControllerService.UserManager.CreateAsync(user, password ?? DefaultPassword);
            if (!result.Succeeded)
            {
                throw new ArgumentException(string.Join("\n", result.Errors.Select(e => e.Description)));
            }

            var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);
            await UserManager.ConfirmEmailAsync(user, token);

            return user;
        }

        public TestServiceHelper MigrateDatabase()
        {
            DbContext.Database.EnsureDeleted();
            DbContext.Database.Migrate();

            return this;
        }

        public ControllerContext LoginClaimControllerContext(ApplicationUser user)
        {
            return new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id)
                    }))
                }
            };
        }

        public DateTimeOffset CurrentDateTime
        {
            get => DateTimeServiceMock._Now;
            set => DateTimeServiceMock._Now = value;
        }

        public TestWebBrowser CreateWebBrowser()
        {
            return new TestWebBrowser(Server);
        }

        public IHtmlDocument ParseHtml(string html)
        {
            var parser = new HtmlParser();
            return parser.Parse(html);
        }
    }
}