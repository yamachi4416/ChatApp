using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatApp.Data;
using ChatApp.Test.Helpers;

namespace ChatApp.Test.IntegrationTests
{

    [CollectionDefinition(TestFixture.CollectionName)]
    public class TestFixture : TestServiceHelper, ICollectionFixture<TestFixture>
    {
        public const string CollectionName = "Integration Test";

        public TestFixture() : base()
        {
            MigrateDatabase();
        }

        public new void Dispose()
        {
            base.Dispose();
        }
    }

    [Collection(TestFixture.CollectionName)]
    public abstract class TestClassBase : IDisposable
    {
        protected readonly TestFixture fixture;

        public TestClassBase(TestFixture fixture)
        {
            this.fixture = fixture;
            fixture.CleanupDatabase();
        }

        protected IEnumerable<ApplicationUser> GetTestUsers()
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

        protected ApplicationUser GetTestUser()
        {
            return GetTestUsers().First();
        }

        protected async Task<ApplicationUser> CreateUserAsync(ApplicationUser user = null)
        {
            return await fixture.CreateUserAsync(user ?? GetTestUser());
        }

        protected async Task<IList<ApplicationUser>> CreateUsersAsync(IEnumerable<ApplicationUser> users)
        {
            var ret = new List<ApplicationUser>();
            foreach (var user in users)
            {
                ret.Add(await CreateUserAsync(user));
            }
            return ret;
        }

        public void Dispose()
        {

        }
    }
}