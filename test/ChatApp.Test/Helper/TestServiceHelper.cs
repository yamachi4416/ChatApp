using Xunit;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ChatApp.Data;
using ChatApp.Services;
using ChatApp.Services.RoomwebSocket;
using ChatApp.Test.Mocks;
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
    public class TestServiceHelper : IDisposable
    {
        public readonly string DefaultPassword = "P@ssw0rd";

        public readonly TestServer Server;

        public T GetService<T>() => Server.Host.Services.GetService<T>();

        public IControllerService ControllerService => GetService<IControllerService>();

        public ApplicationDbContext DbContext => GetService<ApplicationDbContext>();

        public UserManager<ApplicationUser> UserManager => GetService<UserManager<ApplicationUser>>();

        public IRoomWSSender WsSender => GetService<IRoomWSSender>();

        public EmailSenderMock MailSender => GetService<IEmailSender>() as EmailSenderMock; 

        public TestServiceHelper()
        {
            Server = new TestServerBuilder().CreateTestServer();
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user, string password = null)
        {
            var result = await UserManager.CreateAsync(user, password ?? DefaultPassword);
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

        public TestServiceHelper CleanupDatabase()
        {
            DbContext.Database.CreateExecutionStrategy().Execute(DbContext, scope => {
                var querys = string.Join(";", DbContext.Model.GetEntityTypes()
                    .Select(m => $"TRUNCATE TABLE \"{m.Npgsql().TableName}\" CASCADE"));
                scope.Database.ExecuteSqlCommand(querys);
            });

            return this;
        }

        public DateTimeOffset CurrentDateTime
        {
            get => GetService<IDateTimeService>().Now;
            set => (GetService<IDateTimeService>() as DateTimeServiceMock)._Now = value;
        }


        public TestWebBrowser CreateWebBrowser() => new TestWebBrowser(Server);

        public async Task<TestWebBrowser> CreateWebBrowserWithLoginAsyc(ApplicationUser user, string password = null)
        {
            var browser = CreateWebBrowser();
            await browser.GetLoginAsync();
            await browser.TryLoginAsync(user, password ?? DefaultPassword);
            return browser;
        }

        public IHtmlDocument ParseHtml(string html)
        {
            var parser = new HtmlParser();
            return parser.Parse(html);
        }

        public void Dispose()
        {
            Server.Dispose();
            CurrentDateTime = default(DateTimeOffset);
        }
    }
}