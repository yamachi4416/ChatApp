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
            var chatMember = dataCreator.GetChatRoomMember(chatRoom, user);

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
            var chatMember = dataCreator.GetChatRoomMember(chatRoom, user);

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
            var chatMembers = dataCreator.GetChatRoomMembers(chatRooms.Take(2), user).ToList();

            chatMembers[0].IsAdmin = false;
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

        [Fact(DisplayName = "ルームの新しいメッセージを取得できること")]
        public async void RoomApi_GetRoomNewMessages_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var chatRooms = dataCreator.GetChatRooms(user).Take(2).ToList();
            var chatMember = dataCreator.GetChatRoomMember(chatRooms[0], user);

            fixture.DbContext.AddRange(chatRooms);
            fixture.DbContext.AddRange(chatMember);
            await fixture.DbContext.SaveChangesAsync();

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);
            var result = await browser.GetJsonDeserializeResultAsync<IEnumerable<RoomMessageViewModel>>(
                   sitePath[$"/messages/{chatRooms[0].Id}/new"]);

            Assert.Empty(result);

            var messages = dataCreator.GetChatMessages(chatRooms[0], user).Take(40).ToList();
            foreach (var m in messages.Skip(10).Take(8))
            {
                m.ChatRoomId = chatRooms[1].Id.Value;
            }

            fixture.DbContext.AddRange(messages);
            await fixture.DbContext.SaveChangesAsync();

            result = await browser.GetJsonDeserializeResultAsync<IEnumerable<RoomMessageViewModel>>(
                    sitePath[$"/messages/{chatRooms[0].Id}/new"]);

            var expected = messages
                .Where(m => m.ChatRoomId == chatRooms[0].Id)
                .OrderBy(m => m.Id)
                .Select(m => m.Id)
                .TakeLast(30);

            Assert.Equal(30, result.Count());
            Assert.Equal(expected, result.Select(m => m.Id));

            var newMessage = dataCreator.GetChatMessages(chatRooms[0], user).First();
            fixture.DbContext.Add(newMessage);
            await fixture.DbContext.SaveChangesAsync();

            result = await browser.GetJsonDeserializeResultAsync<IEnumerable<RoomMessageViewModel>>(
                    sitePath[$"/messages/{chatRooms[0].Id}/new/{result.Last().Id}"]);

            Assert.Equal(newMessage.Id, result.Single().Id);
        }

        [Fact(DisplayName = "ルームの古いメッセージを取得できること")]
        public async void RoomApi_GetRoomOldMessages_Success()
        {
            var user = await dataCreator.CreateUserAsync();
            var chatRooms = dataCreator.GetChatRooms(user).Take(2).ToList();
            var chatMember = dataCreator.GetChatRoomMembers(chatRooms[0], user);

            fixture.DbContext.AddRange(chatRooms);
            fixture.DbContext.AddRange(chatMember);
            await fixture.DbContext.SaveChangesAsync();

            var messages = dataCreator.GetChatMessages(chatRooms[0], user).Take(40).ToList();
            foreach (var m in messages.Skip(10).Take(8))
            {
                m.ChatRoomId = chatRooms[1].Id.Value;
            }
            fixture.DbContext.AddRange(messages);
            await fixture.DbContext.SaveChangesAsync();

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(user);
            var result = await browser.GetJsonDeserializeResultAsync<IEnumerable<RoomMessageViewModel>>(
                   sitePath[$"/messages/{chatRooms[0].Id}/old/{messages.Last().Id}"]);

            var expected = messages
                .Where(m => m.ChatRoomId == chatRooms[0].Id)
                .OrderByDescending(m => m.Id)
                .Select(m => m.Id)
                .Skip(1)
                .Take(30);

            Assert.Equal(30, result.Count());
            Assert.Equal(expected, result.Select(m => m.Id));

            result = await browser.GetJsonDeserializeResultAsync<IEnumerable<RoomMessageViewModel>>(
                   sitePath[$"/messages/{chatRooms[0].Id}/old/{result.Last().Id}"]);

            Assert.Equal(messages[0].Id, result.Single().Id);
        }

        [Fact(DisplayName = "ルームのメンバーを取得できること")]
        public async void RoomApi_GetRoomMembers_Success()
        {
            var users = await dataCreator.CreateUsersAsync(dataCreator.GetTestUsers().Take(3));

            var chatRooms = dataCreator.GetChatRooms(users[0]).Take(2).ToList();
            var chatMembers = dataCreator.GetChatRoomMembers(chatRooms[0], users).ToList();

            fixture.DbContext.AddRange(chatRooms);
            fixture.DbContext.AddRange(chatMembers);
            await fixture.DbContext.SaveChangesAsync();

            var browser = await fixture.CreateWebBrowserWithLoginAsyc(users[0]);
            var result = await browser.GetJsonDeserializeResultAsync<IEnumerable<RoomMemberViewModel>>(
                sitePath[$"/members/{chatRooms[0].Id}"]);

            Assert.Equal(3, result.Count());
            Assert.Equal(
                users.Select(m => (m.LastName, m.FirstName)).ToHashSet(),
                result.Select(m => (m.LastName, m.FirstName)).ToHashSet());
        }
    }
}
