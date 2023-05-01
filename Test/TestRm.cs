using Skyward.Skygrate.Core;

namespace Test
{
    /// <summary>
    /// rm
    ///     valid
    ///     no matching snapshot
    /// </summary>
    [TestClass]
    public class TestRm
    {
        const string AppName = nameof(TestRm);
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
        public void Remove()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void IncorrectName()
        {
            throw new NotImplementedException();
        }
    }
}