using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Data;

namespace ChatApp.Test.Helpers
{
    public class TestDataCreateHelper
    {
        private readonly TestServiceHelper testHelper;

        public TestDataCreateHelper(TestServiceHelper testHelper)
        {
            this.testHelper = testHelper;
        }

        public IEnumerable<ApplicationUser> GetTestUsers(int startIdx = 1, int count = 100)
        {
            for (int i = startIdx; i < startIdx + count; i++)
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

        public ApplicationUser GetTestUser()
        {
            return GetTestUsers().First();
        }

        public async Task<ApplicationUser> CreateUserAsync(ApplicationUser user = null)
        {
            return await testHelper.CreateUserAsync(user ?? GetTestUser());
        }

        public async Task<IList<ApplicationUser>> CreateUsersAsync(IEnumerable<ApplicationUser> users)
        {
            var ret = new List<ApplicationUser>();
            foreach (var user in users)
            {
                ret.Add(await CreateUserAsync(user));
            }
            return ret;
        }

        public IEnumerable<ChatRoom> GetChatRooms(
            ApplicationUser user, int startIdx = 1, int count = 100)
        {
            for (int i = startIdx; i < startIdx + count; i++)
            {
                var chatRooom = new ChatRoom
                {
                    Name = string.Format("チャットルーム{0,000}", i),
                    Description = string.Format("チャットルーム説明{0,000}", i),
                    CreatedById = user.Id,
                    CreatedDate = testHelper.CurrentDateTime,
                    UpdatedById = user.Id,
                    UpdatedDate = testHelper.CurrentDateTime,
                };

                yield return chatRooom;
            }
        }

        public ChatRoomMember GetChatRoomMember(ChatRoom chatRoom, ApplicationUser user)
        {
            var member = new ChatRoomMember
            {
                ChatRoom = chatRoom,
                UserId = user.Id,
                CreatedById = user.Id,
                CreatedDate = testHelper.CurrentDateTime,
                UpdatedById = user.Id,
                UpdatedDate = testHelper.CurrentDateTime,
            };

            return member;
        }

        public IEnumerable<ChatRoomMember> GetChatRoomMembers(
            ChatRoom chatRoom, ApplicationUser user, int startIdx = 1, int count = 100)
        {
            return Enumerable.Range(startIdx, count)
                .Select(_ => GetChatRoomMember(chatRoom, user));
        }

        public IEnumerable<ChatRoomMember> GetChatRoomMembers(
            ChatRoom chatRoom, IEnumerable<ApplicationUser> users)
        {
            return users.Select(user => GetChatRoomMember(chatRoom, user));
        }

        public IEnumerable<ChatRoomMember> GetChatRoomMembers(
            IEnumerable<ChatRoom> chatRooms, ApplicationUser user)
        {
            return chatRooms.Select(r => GetChatRoomMember(r, user));
        }

        public ChatMessage GetChatMessage(ChatRoom room, ApplicationUser user, string message)
        {
            return new ChatMessage
            {
                ChatRoomId = room.Id.Value,
                UserId = user?.Id,
                Message = string.Format(message),
                CreatedById = user?.Id,
                CreatedDate = testHelper.CurrentDateTime,
                UpdatedById = user?.Id,
                UpdatedDate = testHelper.CurrentDateTime,
            };
        }

        public IEnumerable<ChatMessage> GetChatMessages(
            ChatRoom room, ApplicationUser user, int startIdx = 1, int count = 100)
        {
            return Enumerable.Range(startIdx, count)
                .Select(i => GetChatMessage(room, user, string.Format("チャットメッセージ。{0, 0000}", i)));
        }
    }
}
