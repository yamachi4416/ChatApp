using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ChatApp.Data;
using ChatApp.Features.Room.Models;

namespace ChatApp.Test.IntegrationTests
{
    public class RoomApiAdminTest : TestClassBase
    {
        public RoomApiAdminTest(TestFixture fixture) : base(fixture, "/api/rooms/admin")
        {
        }

        private async Task<ChatRoomMember> CreateAdminMemberWithRoom(ApplicationUser user)
        {
            var room = dataCreator.GetChatRoom(user);
            var member = dataCreator.GetChatRoomMember(room, user);
            member.IsAdmin = true;
            await fixture.DbContext.AddRangeAsync(room, member);
            await fixture.DbContext.SaveChangesAsync();
            return member;
        }

        [Fact(DisplayName = "ルームの管理者はルームにメンバーを追加できること")]
        public async void RoomApiAdmin_AddMember_Success()
        {
            var users = await dataCreator.CreateUsersAsync(dataCreator.GetTestUsers().Take(2));
            var user = users[0];
            var addUser = users[1];
            var member = await CreateAdminMemberWithRoom(user);
            var chatRoom = member.ChatRoom;

            var postModel = new RoomMemberViewModel
            {
                Id = addUser.Id
            };

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);
            await browser.FollowRedirectAsync();

            var result = await browser
                .PostJsonDeserializeResultAsync<RoomMemberViewModel>(
                    sitePath[$"/{chatRoom.Id}/members/add"], postModel);

            Assert.Equal(addUser.Id, result.Id);
            Assert.Equal(addUser.Email, result.Email);
            Assert.Equal(addUser.FirstName, result.FirstName);
            Assert.Equal(addUser.LastName, result.LastName);
            Assert.False(result.IsAdmin);
            Assert.Equal(1, await fixture.DbContext.ChatRoomMembers
                .Where(m => m.UserId == addUser.Id && m.ChatRoomId == chatRoom.Id).CountAsync());

            result = await browser
                .PostJsonDeserializeResultAsync<RoomMemberViewModel>(
                    sitePath[$"/{chatRoom.Id}/members/add"], postModel);

            Assert.Null(result);
            browser.Response.EnsureSuccessStatusCode();
            Assert.Equal(1, await fixture.DbContext.ChatRoomMembers
                .Where(m => m.UserId == addUser.Id && m.ChatRoomId == chatRoom.Id).CountAsync());
        }
    }
}
