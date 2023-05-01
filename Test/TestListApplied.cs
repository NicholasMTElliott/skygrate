using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// list-applied
    ///     no migrations period
    ///     list with all migrations applied
    ///     list invalid chain
    ///     list some migrations not applied
    /// </summary>
    [TestClass]
    public class TestListApplied
    {
        const string AppName = nameof(TestListApplied);
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
        public void NoMigrationsAtAll()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void AllMigrationsApplied()
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
    }
}