using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// add
    ///     no existing migrations
    ///     some migrations
    ///     invalid chain
    /// </summary>
    [TestClass]
    public class TestFreeze
    {
        const string AppName = nameof(TestFreeze);
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
    }
}