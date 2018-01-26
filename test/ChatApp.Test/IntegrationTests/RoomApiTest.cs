using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using ChatApp.Data;
using ChatApp.Test.Helper;
using ChatApp.Features.Room.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Test.IntegrationTests
{
    public class RoomApiTest : IDisposable
    {
        private readonly TestServiceHelper testHelper;

        public RoomApiTest()
        {
            
            testHelper = new TestServiceHelper()
                .MigrateDatabase();
        }

        private IEnumerable<ApplicationUser> GetTestUsers()
        {
            for (int i = 1; ; i++)
            {
                var email = string.Format("testUser-{0,000}@example.com", i);
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = string.Format("名{0,000}", i),
                    LastName = string.Format("姓{0,000}", i),
                };
                yield return user;
            }
        }

        private async Task<IList<ApplicationUser>> CreateUsersAsync(IEnumerable<ApplicationUser> users)
        {
            var ret = new List<ApplicationUser>();
            foreach (var user in users)
            {
                ret.Add(await testHelper.CreateUserAsync(user));
            }
            return ret;
        }

        [Fact(DisplayName = "ユーザがルームを作成できること")]
        public async void RoomApi_CreateRoom_Success()
        {
            var user = (await CreateUsersAsync(GetTestUsers().Take(1))).First();

            var browser = await testHelper.CreateWebBrowserWithLoginAsyc(user);

            testHelper.CurrentDateTime = DateTimeOffset.Parse("2018/01/01");

            var postModel = new RoomViewModel
            {
                Name = "テストルーム",
                Description = "テストルームの説明"
            };

            var actual = await browser.PostJsonDeserializeResultAsync<RoomViewModel>(
                "/chat/api/rooms/rooms/create", postModel);

            Assert.Equal(postModel.Name, actual.Name);
            Assert.Equal(postModel.Description, actual.Description);
            Assert.Equal(testHelper.CurrentDateTime, actual.CreatedDate);
            Assert.Equal(testHelper.CurrentDateTime, actual.UpdatedDate);

            Assert.NotNull(actual.Id);
            Assert.True(actual.IsAdmin);

            var member = await (
                from m in testHelper.DbContext.ChatRoomMembers.AsNoTracking()
                where m.ChatRoomId == actual.Id.Value && m.UserId == user.Id
                select m
            ).SingleOrDefaultAsync();

            Assert.NotNull(member);
            Assert.True(member.IsAdmin);
        }

        public void Dispose()
        {
            testHelper.Dispose();
        }
    }
}