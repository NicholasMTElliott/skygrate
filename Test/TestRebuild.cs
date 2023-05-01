using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// rebase
    ///     valid
    ///     needs fixes
    ///     specified migration
    /// </summary>
    [TestClass]
    public class TestRebuild
    {
        const string AppName = nameof(TestRebuild);
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
        public void AlreadyValid()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void NeedsFixes()
        {
            // NOTE: Rebuild implicitly changes all following migrations as well
            throw new NotImplementedException();
        }


        [TestMethod]
        public void SpecifiedMigration()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void FrozenMigrationsEdited()
        {
            throw new NotImplementedException();
        }
    }
}