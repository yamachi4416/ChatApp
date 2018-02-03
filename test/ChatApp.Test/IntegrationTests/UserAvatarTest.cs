using Xunit;
using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using ChatApp.Features.UserAvatar.Models;
using ChatApp.Data;

namespace ChatApp.Test.IntegrationTests
{
    public class UserAvatarTest : TestClassBase
    {
        public UserAvatarTest(TestFixture fixture) : base(fixture, "/UserAvatar")
        {
        }

        private byte[] GetImageBytes(int width, int height, ImageFormat format)
        {
            using (var img = new Bitmap(width: width, height: height))
            using (var stream = new MemoryStream())
            {
                img.Save(stream, format);
                return stream.ToArray();
            }
        }

        [Fact(DisplayName = "ユーザの画像がない場合デフォルトの画像を返すこと")]
        public async void UserAvatar_DefaultAvatar_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);

            {// アバター画像のIDを指定しない場合
                var resopnse = await browser.GetAsync(sitePath[$"/get"]);
                resopnse.EnsureSuccessStatusCode();

                Assert.Equal("image/png", resopnse.Content.Headers.ContentType.MediaType.ToLowerInvariant());
            }

            {// 存在しないアバター画像のIDを指定した場合
                var resopnse = await browser.GetAsync(sitePath[$"/get/{Guid.NewGuid()}"]);
                resopnse.EnsureSuccessStatusCode();

                Assert.Equal("image/png", resopnse.Content.Headers.ContentType.MediaType.ToLowerInvariant());
            }
        }

        [Fact(DisplayName = "アバター画像のIDを指定してユーザの画像を取得できること")]
        public async void UserAvatar_GetAvatar_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var avatar = new UserAvatar
            {
                UserId = user.Id,
                ContentType = "image/png",
                Content = GetImageBytes(200, 200, ImageFormat.Png)
            };
            fixture.DbContext.Add(avatar);
            await fixture.DbContext.SaveChangesAsync();

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);

            {// アバター画像のIDを指定してアバター画像を取得
                var resopnse = await browser.GetAsync(sitePath[$"/get/{avatar.Id}"]);
                resopnse.EnsureSuccessStatusCode();
                Assert.Equal(avatar.ContentType, resopnse.Content.Headers.ContentType.MediaType.ToLowerInvariant());

                var respAvatar = await resopnse.Content.ReadAsByteArrayAsync();
                Assert.Equal(avatar.Content, respAvatar);
            }

        }

        [Fact(DisplayName = "ユーザがアバター画像をアップロードできること")]
        public async void UserAvatar_Upload_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);
            await browser.FollowRedirectAsync();

            var img = GetImageBytes(200, 200, ImageFormat.Png);
            await browser.PostAsync(sitePath["/upload"], b =>
            {
                b.Multipart(form =>
                {
                    var file = new ByteArrayContent(img);
                    file.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                    form.Add(file, nameof(UploadAvatarModel.ImageFile), "upload.png");
                });
            });

            var result = await browser.DeserializeJsonResultAsync<UserAvatar>();
            Assert.NotNull(result.Id);

            var avatar = await fixture.DbContext.UserAvatars
                .AsNoTracking()
                .SingleAsync(m => m.Id == result.Id);

            Assert.Equal(user.Id, avatar.UserId);
            Assert.Equal("image/png", avatar.ContentType);
            Assert.Equal(img, avatar.Content);

            var updatedUser = await fixture.DbContext.Users
                .AsNoTracking()
                .SingleAsync(m => m.Id == user.Id);

            Assert.Equal(result.Id, updatedUser.UserAvatarId);
        }
    }
}
