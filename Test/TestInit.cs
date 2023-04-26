namespace Test
{
    /// <summary>
    /// init
    ///     nothing
    ///     exists but stopped
    ///     exists
    /// </summary>
    [TestClass]
    public class TestInit
    {
        [TestMethod, TestCategory("Gold Path")]
        public void NothingExistsYet()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void DbExistsButIsStopped()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public void DbExists()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public void DbExistWithDifferentParameters()
        {
            throw new NotImplementedException();
        }
    }
}