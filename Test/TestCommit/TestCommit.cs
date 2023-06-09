﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Skyward.Skygrate.Abstractions;
using Skyward.Skygrate.Core;
using Skyward.Skygrate.MigrationProvider.LocalFileSystem;

namespace Test.TestCommit
{
    /// <summary>
    /// commit
    ///     no pending migrations
    ///     one pending migration
    ///     many pending migrations
    ///     invalid chain
    /// </summary>
    [TestClass]
    public class TestCommit
    {
        const string AppName = nameof(TestCommit);
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
        public void NoPendingMigrations()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public async Task OnePendingMigration()
        {
            var dest = $"{nameof(TestCommit)}\\{nameof(OnePendingMigration)}";
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

            var valid = await logic.ListValidMigrationsAsync();
            valid.Count.ShouldBe(0);
            var pending = await logic.ListPendingMigrationsAsync();
            pending.Count.ShouldBe(1);

            var committed = await logic.CommitPendingMigrationsAsync();
            committed.Count().ShouldBe(1);
            committed.First().Checksum.ShouldNotBeNull();

            valid = await logic.ListValidMigrationsAsync();
            valid.Count.ShouldBe(1);
            pending = await logic.ListPendingMigrationsAsync();
            pending.Count.ShouldBe(0);
        }

        [TestMethod, TestCategory("Gold Path")]
        public async Task ManyPendingMigrations()
        {
            var dest = $"{nameof(TestCommit)}\\{nameof(ManyPendingMigrations)}";
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

            var valid = await logic.ListValidMigrationsAsync();
            valid.Count.ShouldBe(1);
            var pending = await logic.ListPendingMigrationsAsync();
            pending.Count.ShouldBe(2);

            var committed = await logic.CommitPendingMigrationsAsync();
            committed.Count().ShouldBe(2);
            committed.First().Checksum.ShouldNotBeNull();

            valid = await logic.ListValidMigrationsAsync();
            valid.Count.ShouldBe(3);
            pending = await logic.ListPendingMigrationsAsync();
            pending.Count.ShouldBe(0);
        }

        [TestMethod]
        public void InvalidChain()
        {
            throw new NotImplementedException();
        }
    }
}