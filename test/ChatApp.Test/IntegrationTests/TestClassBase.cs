using Xunit;
using System;
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

        protected readonly TestSitePathHelper sitePath;

        protected readonly TestDataCreateHelper dataCreator;

        public TestClassBase(TestFixture fixture, string basePath)
        {
            sitePath = new TestSitePathHelper(basePath);
            dataCreator = new TestDataCreateHelper(fixture);
            this.fixture = fixture;
            fixture.CleanupDatabase();
        }

        public void Dispose()
        {
            fixture.CurrentDateTime = default(DateTimeOffset);
        }
    }
}
