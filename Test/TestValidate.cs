using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// validate
    ///     all valid
    ///     many invalid
    ///     rebase
    ///     rebuild
    /// </summary>
    [TestClass]
    public class TestValidate
    {
        const string AppName = nameof(TestValidate);
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
        public void AllMigrationsValid()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public void ManyInvalidMigrations()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ValidateAndRebase()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void ValidateAndRebuild()
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