using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// list-migrations
    ///     list
    ///     list invalid chain
    ///     list not applied
    ///     no migrations
    /// </summary>
    [TestClass]
    public class TestListMigrations
    {
        const string AppName = nameof(TestListMigrations);
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

        [TestMethod]
        public void NoMigrations()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void SomeMigrationsApplied()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void InvalidChain()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void AllMigrationsApplied()
        {
            throw new NotImplementedException();
        }
    }
}