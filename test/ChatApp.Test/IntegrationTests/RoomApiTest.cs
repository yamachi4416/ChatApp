using Xunit;
using System;
using System.Net;
using System.Linq;
using System.Collections.Generic;
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

        [Fact(DisplayName = "ルームのメンバーはルームにメッセージを投稿できること")]
        public async void RoomApi_MessageCreate_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var chatRoom = dataCreator.GetChatRooms(user).First();
            var chatMember = dataCreator.GetChatRoomMembers(chatRoom, user).First();

            fixture.DbContext.AddRange(chatRoom, chatMember);
            await fixture.DbContext.SaveChangesAsync();

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);

            var postMessage = new PostMessageModel
            {
                Message = "こんにちは。"
            };

            fixture.CurrentDateTime = DateTimeOffset.Parse("2018/01/01");
            var result = await browser
                .PostJsonDeserializeResultAsync<RoomMessageViewModel>(
                    sitePath[$"/messages/{chatRoom.Id}/create"], postMessage);

            Assert.True(result.Id.HasValue);
            Assert.Equal(postMessage.Message, result.Message);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(user.FirstName, result.UserFirstName);
            Assert.Equal(user.LastName, result.UserLastName);
            Assert.Equal(fixture.CurrentDateTime, result.CreatedDate);
            Assert.Equal(fixture.CurrentDateTime, result.UpdatedDate);

            Assert.NotNull(await fixture.DbContext.ChatMessages
                .Where(m => m.ChatRoomId == chatRoom.Id && m.UserId == user.Id)
                .FirstOrDefaultAsync());
        }

        [Fact(DisplayName = "メッセージが空の場合バリデーションエラーになること")]
        public async void RoomApi_MessageCreate_Validation_Failure()
        {
            var user = await dataCreator.CreateUserAsync();
            var chatRoom = dataCreator.GetChatRooms(user).First();
            var chatMember = dataCreator.GetChatRoomMembers(chatRoom, user).First();

            fixture.DbContext.AddRange(chatRoom, chatMember);
            await fixture.DbContext.SaveChangesAsync();

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);

            var postMessage = new PostMessageModel
            {
                Message = ""
            };

            await browser.PostJsonAsync(sitePath[$"/messages/{chatRoom.Id}/create"], postMessage);
            var result = await browser.DeserializeApiErrorJsonResultAsync();

            Assert.Equal(1, result.Count);
            Assert.Contains(nameof(postMessage.Message).ToLowerInvariant(), result.Keys);
            Assert.Empty(await fixture.DbContext.ChatMessages.ToListAsync());
        }

        [Fact(DisplayName = "メンバーになっているルーム情報を取得できること")]
        public async void RoomApi_GetJoinRooms_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var chatRooms = dataCreator.GetChatRooms(user).Take(3).ToList();
            var chatMembers = dataCreator.GetChatRoomMembers(null, user).Take(2).ToList();

            chatMembers[0].ChatRoom = chatRooms[0];
            chatMembers[0].IsAdmin = false;
            chatMembers[1].ChatRoom = chatRooms[1];
            chatMembers[1].IsAdmin = true;

            fixture.DbContext.AddRange(chatRooms);
            fixture.DbContext.AddRange(chatMembers);
            await fixture.DbContext.SaveChangesAsync();

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);
            var result = (await browser
                .GetJsonDeserializeResultAsync<IEnumerable<RoomViewModel>>(sitePath["/joins"]))
                .ToList();

            Assert.Equal(2, result.Count());

            var eRoom = chatRooms[0];
            var aRoom = result.Where(m => m.Id == eRoom.Id).SingleOrDefault();
            Assert.Equal(eRoom.Name, aRoom.Name);
            Assert.Equal(eRoom.Description, aRoom.Description);
            Assert.Equal(false, aRoom.IsAdmin);

            eRoom = chatRooms[1];
            aRoom = result.Where(m => m.Id == eRoom.Id).SingleOrDefault();
            Assert.Equal(eRoom.Name, aRoom.Name);
            Assert.Equal(eRoom.Description, aRoom.Description);
            Assert.Equal(true, aRoom.IsAdmin);
        }
    }
}
