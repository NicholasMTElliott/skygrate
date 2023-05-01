using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// list-snapshots
    ///     list
    ///     no snapshots
    /// </summary>
    [TestClass]
    public class TestListSnapshots
    {
        const string AppName = nameof(TestListSnapshots);
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
        public void NoSnapshots()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void SnapshotsExist()
        {
            throw new NotImplementedException();
        }
    }
}