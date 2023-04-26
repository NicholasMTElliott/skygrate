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