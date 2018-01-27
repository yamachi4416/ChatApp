using Xunit;
using System;
using System.Net;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ChatApp.Features.Room.Models;

namespace ChatApp.Test.IntegrationTests
{
    public class RoomApiTest : TestClassBase
    {
        public RoomApiTest(TestFixture fixture) : base(fixture, "/api/rooms")
        {
        }

        [Fact(DisplayName = "ユーザがルームを作成できること")]
        public async void RoomApi_CreateRoom_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);

            fixture.CurrentDateTime = DateTimeOffset.Parse("2018/01/01");

            var postModel = new RoomViewModel
            {
                Name = "テストルーム",
                Description = "テストルームの説明"
            };

            var actual = await browser.PostJsonDeserializeResultAsync<RoomViewModel>(
                sitePath["/rooms/create"], postModel);

            Assert.Equal(postModel.Name, actual.Name);
            Assert.Equal(postModel.Description, actual.Description);
            Assert.Equal(fixture.CurrentDateTime, actual.CreatedDate);
            Assert.Equal(fixture.CurrentDateTime, actual.UpdatedDate);

            Assert.NotNull(actual.Id);
            Assert.True(actual.IsAdmin);

            var member = await (
                from m in fixture.DbContext.ChatRoomMembers.AsNoTracking()
                where m.ChatRoomId == actual.Id.Value && m.UserId == user.Id
                select m
            ).SingleOrDefaultAsync();

            Assert.NotNull(member);
            Assert.True(member.IsAdmin);
        }

        [Fact(DisplayName = "ルーム名が未入力の場合バリデーションエラーになること")]
        public async void RoomApi_CreateRoom_Validation_Failure()
        {
            var user = await dataCreator.CreateUserAsync();
            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);

            var postModel = new RoomViewModel
            {
                Name = "",
                Description = ""
            };

            var result = await browser.PostJsonAsync(sitePath["/rooms/create"], postModel);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);

            var errors = await browser.DeserializeApiErrorJsonResultAsync();

            Assert.Equal(1, errors.Count);
            Assert.Contains(nameof(postModel.Name).ToLowerInvariant(), errors.Keys);

            Assert.Empty(await fixture.DbContext.ChatRooms.ToListAsync());
        }
    }
}
