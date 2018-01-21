using Xunit;
using Moq;
using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using ChatApp.Data;
using ChatApp.Test.Mocks;
using ChatApp.Test.Helper;
using ChatApp.Test.Attrubutes;
using ChatApp.Features.Account;
using ChatApp.Features.Account.Models;

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

        [Fact(DisplayName = "不正なパスワードの場合はアカウントの登録ができないこと")]
        public async void Account_Regiter_InvalidPassword_Failure()
        {
            var user = GetTestUser();
            var browser = testHelper.CreateWebBrowser();

            // アカウントの登録画面を開けること
            await browser.GetAsync("/chat/Account/Register");
            browser.Response.EnsureSuccessStatusCode();

            // アカウントの登録が失敗すること
            await browser.PostAsync("/chat/Account/Register", b =>
            {
                b.Form(form =>
                {
                    form.Add("LastName", user.LastName);
                    form.Add("FirstName", user.FirstName);
                    form.Add("Email", user.Email);
                    form.Add("Password", "Password");
                    form.Add("ConfirmPassword", "Password");
                });
            });
            browser.Response.EnsureSuccessStatusCode();

            var describer = testHelper.UserManager.ErrorDescriber;
            var errors = testHelper.ParseHtml(await browser.Response.Content.ReadAsStringAsync())
                .QuerySelectorAll(".validation-summary-errors li")
                .Select(m => m.TextContent);

            // パスワードには、少なくとも1つの英数字以外の文字が必要です。
            Assert.Contains(describer.PasswordRequiresNonAlphanumeric().Description, errors);
            // パスワードには少なくとも1桁の数字（'0'〜 '9'）が必要です。
            Assert.Contains(describer.PasswordRequiresDigit().Description, errors);

            // ログインできないこと
            Assert.False(await TryLogin(browser, user));
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
            await browser.PostAsync(resetUrl, b =>
            {
                b.Form(form =>
                {
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

        [Fact(DisplayName = "GoogleでログインでGoogleの認証ページにリダイレクトされること")]
        public async void Account_ExternalLogin_Redirect_Google_Success()
        {
            var service = testHelper.ControllerService;
            var browser = testHelper.CreateWebBrowser();

            await browser.GetAsync("/chat/Account/Login");
            await browser.PostAsync("/chat/Account/ExternalLogin", b =>
            {
                b.Form(form =>
                {
                    form.Add("provider", "Google");
                });
            });

            var result = browser.Response;

            Assert.Equal(HttpStatusCode.Redirect, browser.Response.StatusCode);
            Assert.Equal("accounts.google.com", browser.Response.Headers.Location.Host);
        }

        [Fact(DisplayName = "Googleで認証をするとユーザがログインできるようになること")]
        public async void Account_Google_ExternalLoginCallback_UserLogin_Success()
        {
            var service = testHelper.ControllerService;

            var user = GetTestUser();
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.GivenName, user.FirstName)
            }));

            var signInManager = new Mock<SignInManagerMock>(service.UserManager);
            signInManager
                .Setup(m => m.GetExternalLoginInfoAsync(It.IsAny<string>()))
                .ReturnsAsync(new ExternalLoginInfo(claims,
                    GoogleDefaults.DisplayName, GoogleDefaults.AuthenticationScheme, GoogleDefaults.DisplayName));
            signInManager
                .Setup(m => m.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var urlHelper = new Mock<UrlHelperMock>();
            urlHelper.SetupGet(m => m._isLocalUrl).Returns(true);

            var controller = new AccountController(service, signInManager.Object, testHelper.MailSender)
            {
                Url = urlHelper.Object
            };

            var result = await controller.ExternalLoginCallback("/chat") as RedirectResult;

            Assert.Equal("/chat", result.Url);

            var createdUser = await (
                from m in testHelper.DbContext.Users
                where m.UserName == user.Email
                select m).FirstOrDefaultAsync();

            Assert.NotNull(createdUser);
            Assert.Equal(user.FirstName, createdUser.FirstName);
            Assert.Equal(user.LastName, createdUser.LastName);
            Assert.True(createdUser.EmailConfirmed);

            var userLogin = await (
                from m in testHelper.DbContext.UserLogins
                where m.UserId == createdUser.Id
                   && m.ProviderDisplayName == GoogleDefaults.DisplayName
                select m).FirstOrDefaultAsync();

            Assert.NotNull(userLogin);
        }

        [Fact(DisplayName = "Googleで認証をして名前が未設定の場合は確認ページに遷移すること")]
        public async void Account_Google_ExternalLoginCallback_RedirectTo_ConfirmPage_Success()
        {
            var service = testHelper.ControllerService;

            var user = GetTestUser();
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Surname, user.LastName)
            }));

            var signInManager = new Mock<SignInManagerMock>(service.UserManager);
            signInManager
                .Setup(m => m.GetExternalLoginInfoAsync(It.IsAny<string>()))
                .ReturnsAsync(new ExternalLoginInfo(claims,
                    GoogleDefaults.DisplayName, GoogleDefaults.AuthenticationScheme, GoogleDefaults.DisplayName));
            signInManager
                .Setup(m => m.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var controller = new AccountController(service, signInManager.Object, testHelper.MailSender);
            var result = await controller.ExternalLoginCallback(returnUrl: "/chat") as ViewResult;

            Assert.Equal(nameof(controller.ExternalLoginConfirmation), result.ViewName);
            Assert.Equal(GoogleDefaults.DisplayName, result.ViewData["LoginProvider"] as string);
            Assert.Equal("/chat", result.ViewData["ReturnUrl"] as string);

            var model = result.Model as ExternalLoginConfirmationViewModel;
            Assert.True(string.IsNullOrEmpty(model.FirstName));
            Assert.Equal(user.LastName, model.LastName);
        }

        [Fact(DisplayName = "Googleでログインがすでに設定されている場合はログインできること")]
        public async void Account_Google_ExternalLoginCallback_ExistsLogin_Success()
        {
            var service = testHelper.ControllerService;

            var user = GetTestUser();
            var claims = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Email, user.Email)
            }));

            var signInManager = new Mock<SignInManagerMock>(service.UserManager);
            signInManager
                .Setup(m => m.GetExternalLoginInfoAsync(It.IsAny<string>()))
                .ReturnsAsync(new ExternalLoginInfo(claims,
                    GoogleDefaults.DisplayName, GoogleDefaults.AuthenticationScheme, GoogleDefaults.DisplayName));
            signInManager
                .Setup(m => m.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var controller = new AccountController(service, signInManager.Object, testHelper.MailSender)
            {
                Url = Mock.Of<UrlHelperMock>(m => m._isLocalUrl == true)
            };

            var result = await controller.ExternalLoginCallback("/chat") as RedirectResult;
            Assert.Equal("/chat", result.Url);
        }

        public void Dispose()
        {
            testHelper.Server.Dispose();
        }
    }
}