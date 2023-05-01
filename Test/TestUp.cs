using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// up
    ///     default
    ///     targeted
    ///     partially ready vs from scratch
    ///     abort (invalid chain)
    ///     rebase
    ///     rebuild
    ///     base
    ///     pretend
    /// </summary>
    [TestClass]
    public class TestUp
    {
        const string AppName = nameof(TestUp);
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
        public void FromScratch()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public void SomeMigrationsApplied()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TargetForward()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public void TargetBackward()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public void InvalidChain()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void WithRebuild()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void WithRebase()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void BasedOnSnapshot()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void WithPretend()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public void MigrationFails()
        {
            throw new NotImplementedException();
        }
    }
}