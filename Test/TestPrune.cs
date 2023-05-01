using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// prune
    ///     no orphans
    ///     some orphans
    ///        forced all
    /// </summary>
    [TestClass]
    public class TestPrune
    {
        const string AppName = nameof(TestPrune);
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
        public void NoOrphans()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void SomeOrphans()
        {
            throw new NotImplementedException();
        }


        [TestMethod]
        public void ForceAll()
        {
            throw new NotImplementedException();
        }
    }
}