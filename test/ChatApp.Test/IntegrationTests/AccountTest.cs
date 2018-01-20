using Xunit;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ChatApp.Data;
using ChatApp.Test.Helper;
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

        private ApplicationUser GetTestUser()
        {
            return new ApplicationUser
            {
                UserName = "testUser-01@example.com",
                Email = "testUser-01@example.com",
                FirstName = "テスト",
                LastName = "一郎"
            };
        }

        private async Task<ApplicationUser> CreateUserAsync()
        {
            return await testHelper.CreateUserAsync(GetTestUser());
        }

        private async Task<bool> TryLogin(TestWebBrowser browser, ApplicationUser user, string password = null)
        {
            await browser.PostAsync("/chat/Account/Login", b =>
            {
                b.Form(form =>
                {
                    form.Add("Email", user.Email);
                    form.Add("Password", password ?? testHelper.DefaultPassword);
                });
            });

            return browser.Response.StatusCode == HttpStatusCode.Redirect
                && browser.Response.Headers.Location.OriginalString == "/chat";
        }

        [Fact(DisplayName = "アカウントを作成してメールの確認で有効化されること")]
        public async void Account_Regiter_Email_Confirmation_Success()
        {
            var user = GetTestUser();
            var browser = testHelper.CreateWebBrowser();

            // アカウントの登録画面を開けること
            await browser.GetAsync("/chat/Account/Register");
            browser.Response.EnsureSuccessStatusCode();

            // アカウントの登録ができること
            await browser.PostAsync("/chat/Account/Register", b =>
            {
                b.Form(form =>
                {
                    form.Add("LastName", user.LastName);
                    form.Add("FirstName", user.FirstName);
                    form.Add("Email", user.Email);
                    form.Add("Password", testHelper.DefaultPassword);
                    form.Add("ConfirmPassword", testHelper.DefaultPassword);
                });
            });

            Assert.Equal(HttpStatusCode.Redirect, browser.Response.StatusCode);
            Assert.Equal("/chat", browser.Response.Headers.Location.OriginalString);

            // メールの確認前はログインできないこと
            Assert.False(await TryLogin(browser, user));

            // ユーザにメールが送信されていること
            var mail = testHelper.MailSender.GetLastMessage;
            Assert.Equal(user.Email, mail.To);

            // 確認メールのリンクのからメールの確認ができること
            var confirmUrl = testHelper.ParseHtml(mail.Body)
                .GetElementsByTagName("a")
                .Select(a => a.GetAttribute("href"))
                .FirstOrDefault();

            await browser.GetAsync(confirmUrl);
            browser.Response.EnsureSuccessStatusCode();

            // メールに確認後はログインができること
            Assert.True(await TryLogin(browser, user));
        }

        [Fact(DisplayName = "ログイン・ログアウトが正常にできること")]
        public async void Account_Login_Logoff_Success()
        {
            var user = await CreateUserAsync();
            var browser = testHelper.CreateWebBrowser();

            // 登録済みのユーザでログインできること
            await browser.GetAsync("/chat/Account/Login");
            browser.Response.EnsureSuccessStatusCode();
            Assert.True(await TryLogin(browser, user));

            // ログアウトができること
            await browser.FollowRedirect();
            await browser.PostAsync("/chat/Account/LogOff");
            Assert.Equal(HttpStatusCode.Redirect, browser.Response.StatusCode);
            Assert.Equal("/chat", browser.Response.Headers.Location.OriginalString);

            // ログアウト後はログインできないこと
            Assert.False(await TryLogin(browser, user));
        }

        [Fact(DisplayName = "パスワードの変更をすることができること")]
        public async void Account_Change_Password_Success()
        {
            var user = await CreateUserAsync();
            var browser = testHelper.CreateWebBrowser();

            await browser.GetAsync("/chat/Account/ForgotPassword");
            browser.Response.EnsureSuccessStatusCode();

            await browser.PostAsync("/chat/Account/ForgotPassword", b =>
            {
                b.Form(form =>
                {
                    form.Add("Email", user.Email);
                });
            });
            browser.Response.EnsureSuccessStatusCode();

            // ユーザにメールが送信されていること
            var mail = testHelper.MailSender.GetLastMessage;
            Assert.Equal(user.Email, mail.To);

            // 変更メールのリンクのからメールの変更ができること
            var resetUrl = testHelper.ParseHtml(mail.Body)
                .GetElementsByTagName("a")
                .Select(a => a.GetAttribute("href"))
                .FirstOrDefault();

            await browser.GetAsync(resetUrl);
            browser.Response.EnsureSuccessStatusCode();

            var newPassword = $"{testHelper.DefaultPassword}A";
            await browser.PostAsync(resetUrl, b => {
                b.Form(form => {
                    form.Add("Email", user.Email);
                    form.Add("Password", newPassword);
                    form.Add("ConfirmPassword", newPassword);
                });
            });
            Assert.Equal(HttpStatusCode.Redirect, browser.Response.StatusCode);
            Assert.Equal("/chat/Account/ResetPasswordConfirmation", browser.Response.Headers.Location.OriginalString);
            await browser.FollowRedirect();

            // 新しいパスワードでログインできること
            Assert.True(await TryLogin(browser, user, newPassword));
        }

        public void Dispose()
        {
            testHelper.Server.Dispose();
        }
    }
}