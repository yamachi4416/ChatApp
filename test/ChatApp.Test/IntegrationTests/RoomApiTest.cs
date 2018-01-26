using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ChatApp.Data;
using ChatApp.Features.Room.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Test.IntegrationTests
{
    public class RoomApiTest : TestClassBase
    {
        public RoomApiTest(TestFixture fixture) : base(fixture)
        {
        }

        [Fact(DisplayName = "ユーザがルームを作成できること")]
        public async void RoomApi_CreateRoom_Success()
        {
            var user = await CreateUserAsync();

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);

            fixture.CurrentDateTime = DateTimeOffset.Parse("2018/01/01");

            var postModel = new RoomViewModel
            {
                Name = "テストルーム",
                Description = "テストルームの説明"
            };

            var actual = await browser.PostJsonDeserializeResultAsync<RoomViewModel>(
                "/chat/api/rooms/rooms/create", postModel);

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
    }
}