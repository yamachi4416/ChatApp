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

        [Fact(DisplayName = "ルームの管理者はルームにメンバーを削除できること")]
        public async void RoomApiAdmin_RemoveRoom_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var chatRooms = dataCreator.GetChatRooms(user).Take(2).ToList();

            var users = dataCreator.GetTestUsers().Skip(1).Take(2).ToList();
            await fixture.DbContext.AddRangeAsync(users);
            await fixture.DbContext.SaveChangesAsync();

            var adminMember = dataCreator.GetChatRoomMember(chatRooms[0], user);
            adminMember.IsAdmin = true;
            var member1 = dataCreator.GetChatRoomMember(chatRooms[0], users[0]);
            var member2 = dataCreator.GetChatRoomMember(chatRooms[1], users[1]);

            await fixture.DbContext.AddRangeAsync(chatRooms);
            await fixture.DbContext.AddRangeAsync(adminMember, member1, member2);
            await fixture.DbContext.SaveChangesAsync();

            var requestPath = sitePath[$"/{chatRooms[0].Id}/members/remove"];
            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);
            await browser.FollowRedirectAsync();

            // ルームのメンバーを削除できること
            var result = await browser.PostJsonDeserializeResultAsync<RoomMemberViewModel>(
                requestPath, new RoomMemberViewModel { Id = member1.UserId });

            Assert.Equal(member1.UserId, result.Id);
            Assert.Empty(await fixture.DbContext.ChatRoomMembers
                .Where(m => m.UserId == member1.UserId)
                .ToListAsync());

            // ほかのルームのメンバーは削除できないこと
            result = await browser.PostJsonDeserializeResultAsync<RoomMemberViewModel>(
                requestPath, new RoomMemberViewModel { Id = member2.UserId });

            Assert.Null(result);
            browser.Response.EnsureSuccessStatusCode();
            Assert.NotEmpty(await fixture.DbContext.ChatRoomMembers
                .Where(m => m.UserId == member2.UserId).ToListAsync());

            // 管理者は削除できないこと
            result = await browser.PostJsonDeserializeResultAsync<RoomMemberViewModel>(
                requestPath, new RoomMemberViewModel { Id = adminMember.UserId });

            Assert.Null(result);
            browser.Response.EnsureSuccessStatusCode();
            Assert.NotEmpty(await fixture.DbContext.ChatRoomMembers
                .Where(m => m.UserId == adminMember.UserId).ToListAsync());
        }
    }
}
