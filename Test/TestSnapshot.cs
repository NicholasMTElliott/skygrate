using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// snapshot
    ///     valid
    ///     no instance
    /// </summary>
    [TestClass]
    public class TestSnapshot
    {
        const string AppName = nameof(TestSnapshot);
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
        public void Valid()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void NoRunningInstance()
        {
            throw new NotImplementedException();
        }
    }
}