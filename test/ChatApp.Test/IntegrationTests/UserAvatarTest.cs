using Xunit;
using System;
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
                Content = new byte[] { 1 }
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

            var img = new byte[] { 1, 2, 3 };
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

        [Fact(DisplayName = "ユーザがアバター画像をアップロードして更新できること")]
        public async void UserAvatar_Upload_Update_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var existsAvatar = new UserAvatar
            {
                UserId = user.Id,
                Content = new byte[] { 1, 2, 3 },
                ContentType = "image/png"
            };

            await fixture.DbContext.AddAsync(existsAvatar);
            await fixture.DbContext.SaveChangesAsync();

            var uploadImage = new byte[] { 255, 255, 255 };

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);
            await browser.FollowRedirectAsync();
            await browser.PostAsync(sitePath["/upload"], b =>
            {
                b.Multipart(form =>
                {
                    var file = new ByteArrayContent(uploadImage);
                    file.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                    form.Add(file, nameof(UploadAvatarModel.ImageFile), "upload.png");
                });
            });

            var result = await browser.DeserializeJsonResultAsync<UserAvatar>();

            // 事前確認
            Assert.NotNull(existsAvatar.Id);
            Assert.NotEqual(uploadImage, existsAvatar.Content);
            Assert.NotEqual(existsAvatar.Id, result.Id);

            // ユーザの更新前のアバターが削除されていること
            Assert.Null(await fixture.DbContext.UserAvatars
                .AsNoTracking()
                .SingleOrDefaultAsync(m => m.Id == existsAvatar.Id));

            // アップロードした画像でユーザのアバターが作成されていること
            var newAvatar = await fixture.DbContext.UserAvatars
                .AsNoTracking()
                .SingleAsync(m => m.Id == result.Id);
            Assert.Equal(user.Id, newAvatar.UserId);
            Assert.Equal(uploadImage, newAvatar.Content);
            Assert.Equal("image/png", newAvatar.ContentType);

            // ユーザに更新後のアバターのIDが設定されていること
            var updatedUser = await fixture.DbContext.Users
                .AsNoTracking()
                .SingleAsync(m => m.Id == user.Id);
            Assert.Equal(result.Id, updatedUser.UserAvatarId);
        }

        [Fact(DisplayName = "ユーザが不正な画像をアップロードした場合バリデーションエラーになること")]
        public async void UserAvatar_Upload_Validation_Failure()
        {
            var user = await dataCreator.CreateUserAsync();
            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);
            await browser.FollowRedirectAsync();

            {// ファイルを指定しない場合
                await browser.PostAsync(sitePath["/upload"]);
                var result = await browser.DeserializeApiErrorJsonResultAsync();
                Assert.NotEmpty(result);
            }

            {// ファイルサイズが大きすぎる場合
                var img = new byte[300001];

                await browser.PostAsync(sitePath["/upload"], b =>
                {
                    b.Multipart(form =>
                    {
                        var file = new ByteArrayContent(img);
                        file.Headers.ContentType = MediaTypeHeaderValue.Parse("image/png");
                        form.Add(file, nameof(UploadAvatarModel.ImageFile), "upload.png");
                    });
                });

                // レスポンスの確認
                var result = await browser.DeserializeApiErrorJsonResultAsync();
                Assert.NotEmpty(result);
                Assert.Contains(nameof(UserAvatar.Content), result.Keys);

                // データベースの確認
                var avatar = await fixture.DbContext.UserAvatars
                    .AsNoTracking()
                    .SingleOrDefaultAsync(m => m.UserId == user.Id);

                Assert.Null(avatar);
            }
        }
    }
}
