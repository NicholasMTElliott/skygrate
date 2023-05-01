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
    public class TestRebase
    {
        const string AppName = nameof(TestRebase);
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
        public void AlreadyValid()
        {
            throw new NotImplementedException();
        }


        [TestMethod, TestCategory("Gold Path")]
        public void NeedsFixes()
        {
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