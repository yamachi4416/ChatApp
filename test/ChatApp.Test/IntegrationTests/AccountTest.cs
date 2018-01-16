using ChatApp.Data;
using Xunit;
using System;
using ChatApp.Test.Helper;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net;
using ChatApp.Test.Attrubutes;

namespace ChatApp.Test.IntegrationTests
{
    [IntegrationTest]
    public class AccountTest : IDisposable
    {
        private readonly TestServiceHelper testHelper;

        public AccountTest()
        {
            testHelper = new TestServiceHelper()
                .MigrateDatabase();
        }

        private async Task<ApplicationUser> CreateUserAsync()
        {
            return await testHelper.CreateUserAsync(new ApplicationUser
            {
                UserName = "testUser-01@example.com",
                Email = "testUser-01@example.com",
                FirstName = "テスト",
                LastName = "一郎"
            });
        }

        [Fact(DisplayName = "ログイン・ログアウトが正常にできること")]
        public async void Account_Login_Logoff_Success()
        {
            var user = await CreateUserAsync();
            var browser = testHelper.CreateWebBrowser();

            await browser.GetAsync("/chat/Account/Login");
            browser.Response.EnsureSuccessStatusCode();

            await browser.PostAsync("/chat/Account/Login", b => {
                b.Form(form => {
                    form.Add("Email", user.Email);
                    form.Add("Password", testHelper.DefaultPassword);
                });
            });
            
            Assert.Equal(HttpStatusCode.Redirect, browser.Response.StatusCode);
            Assert.Equal("/chat", browser.Response.Headers.Location.OriginalString);

            await browser.FollowRedirect();
            await browser.PostAsync("/chat/Account/LogOff");
            
            Assert.Equal(HttpStatusCode.Redirect, browser.Response.StatusCode);
            Assert.Equal("/chat", browser.Response.Headers.Location.OriginalString);
        }

        public void Dispose()
        {
            testHelper.Server.Dispose();
        }
    }
}