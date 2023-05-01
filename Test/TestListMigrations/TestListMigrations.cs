using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Skyward.Skygrate.Abstractions;
using Skyward.Skygrate.Core;
using Skyward.Skygrate.MigrationProvider.LocalFileSystem;

namespace Test
{
    /// <summary>
    /// list-migrations
    ///     list
    ///     list invalid chain
    ///     list not applied
    ///     no migrations
    /// </summary>
    [TestClass]
    public class TestListMigrations
    {
        const string AppName = nameof(TestListMigrations);
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
        public async Task AllCombinations()
        {
            var dest = $"{nameof(TestListMigrations)}\\{nameof(AllCombinations)}";
            var source = $".\\{dest}";
            var root = TestUtil.CloneTestData(source, dest);
            Directory.Exists(root).ShouldBeTrue();

            var dbProviderMock = new Mock<IDatabaseProvider>(MockBehavior.Strict);
            var loggerFactory = new NullLoggerFactory();
            var logic = new MigrationLogic(
                Options,
                dbProviderMock.Object,
                new LocalFileSystemMigrationProvider(loggerFactory.CreateLogger<LocalFileSystemMigrationProvider>(), new OptionsWrapper<Config>(new Config
                {
                    BasePath = root
                })),
                loggerFactory.CreateLogger<MigrationLogic>()
                );

            var allMigrationsWithStatuses = await logic.ListMigrationsWithStatus();
            allMigrationsWithStatuses[0].status.ShouldBe(MigrationStatus.Changed);
            allMigrationsWithStatuses[1].status.ShouldBe(MigrationStatus.ValidChain);
            allMigrationsWithStatuses[2].status.ShouldBe(MigrationStatus.ValidChain);
            allMigrationsWithStatuses[3].status.ShouldBe(MigrationStatus.InvalidWithin);
            allMigrationsWithStatuses[4].status.ShouldBe(MigrationStatus.Changed);
            allMigrationsWithStatuses[5].status.ShouldBe(MigrationStatus.ValidChain);
            allMigrationsWithStatuses[6].status.ShouldBe(MigrationStatus.InvalidAfter);
            allMigrationsWithStatuses[7].status.ShouldBe(MigrationStatus.Pending);
            allMigrationsWithStatuses[8].status.ShouldBe(MigrationStatus.Changed);
        }

        [TestMethod]
        public void NoMigrations()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void SomeMigrationsApplied()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void InvalidChain()
        {
            throw new NotImplementedException();
        }
        [TestMethod]
        public void AllMigrationsApplied()
        {
            throw new NotImplementedException();
        }
    }
}