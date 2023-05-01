using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// rollback
    ///     no migrations
    ///     one migration
    ///     many migrations
    ///     many migration, targetted migration
    ///     many migration, invalid id or name
    /// </summary>
    [TestClass]
    public class TestRollback
    {
        const string AppName = nameof(TestRollback);
        readonly static LaunchOptions Options = new LaunchOptions
        {
            ApplicationName = AppName,
        };


        [TestInitialize]
        public async Task TestInitialize()
        {
            await TestUtil.Terminate(Options);
        }

        [TestCleanup()]
        public async Task TestCleanup()
        {
            await TestUtil.Terminate(Options);
        }

        [TestMethod, TestCategory("Gold Path")]
        public void NoMigrations()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public void OneMigration()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public void ManyMigrations()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void ManyMigrationsWithTarget()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void ManyMigrationsWithInvalidTarget()
        {
            throw new NotImplementedException();
        }
    }
}