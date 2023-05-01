using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Skygrate.DatabaseProvider.Postgresql;
using Skyward.Skygrate.Abstractions;
using Skyward.Skygrate.Core;
using Skyward.Skygrate.MigrationProvider.LocalFileSystem;

namespace Test.TestAdd
{
    /// <summary>
    /// add
    ///     no existing migrations
    ///     some migrations
    ///     duplicate name warning
    ///     invalid characters
    /// </summary>
    [TestClass]
    public class TestAdd
    {
        const string AppName = nameof(TestAdd);
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

        /// <summary>
        /// Scenario: No migrations exist yet, so this is a special-case for the first-ever migration.
        /// </summary>
        [TestMethod, TestCategory("Gold Path")]
        public async Task NoPriorMigrations()
        {
            var dest = $"{nameof(TestAdd)}\\{nameof(NoPriorMigrations)}";
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


            var continuation = await logic.AddNewMigrationAsync(nameof(NoPriorMigrations));
            continuation.Resolved.ShouldBeTrue();
            MigrationReference migration = continuation.Value!.Value;
            migration.Id.ShouldNotBeNullOrWhiteSpace();
            migration.Id.Length.ShouldBe(8);
            migration.Timestamp.ShouldNotBeNullOrWhiteSpace();
            migration.Timestamp.Length.ShouldBe(14);
            migration.Checksum.ShouldBeNull();
            migration.Name.ShouldBe(nameof(NoPriorMigrations));
            migration.PriorId.ShouldBeNull();

            File.Exists($"{root}\\{migration.Timestamp}_{migration.Id}_{migration.Name}.sql").ShouldBeTrue();
        }

        /// <summary>
        /// Scenario: There is at least one prior migration, so this should correctly create a new migration
        /// that is in the chain after the most recent existing one.
        /// There are two migrations added here so we can verify it is correctly placed after the second one.
        /// </summary>
        [TestMethod, TestCategory("Gold Path")]
        public async Task SomePriorMigrations()
        {
            var dest = $"{nameof(TestAdd)}\\{nameof(SomePriorMigrations)}";
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


            var continuation = await logic.AddNewMigrationAsync(nameof(SomePriorMigrations));
            continuation.Resolved.ShouldBeTrue();
            MigrationReference migration = continuation.Value!.Value;
            migration.Id.ShouldNotBeNullOrWhiteSpace();
            migration.Id.Length.ShouldBe(8);
            migration.Timestamp.ShouldNotBeNullOrWhiteSpace();
            migration.Timestamp.Length.ShouldBe(14);
            migration.Checksum.ShouldBeNull();
            migration.Name.ShouldBe(nameof(SomePriorMigrations));
            migration.PriorId.ShouldBe("12345679"); // 12345678 means it referenced the _first_ migration

            File.Exists($"{root}\\{migration.Timestamp}_{migration.Id}_{migration.Name}.sql").ShouldBeTrue();
        }

        [TestMethod]
        public async Task DuplicateNameSpecified()
        {
            await TestUtil.Terminate(Options);

            var root = $".\\{nameof(TestAdd)}\\{nameof(DuplicateNameSpecified)}";
            Directory.Exists(root).ShouldBeTrue();
            throw new NotImplementedException();
        }

        [TestMethod]
        public async Task InvalidCharacters()
        {
            await TestUtil.Terminate(Options);

            var root = $".\\{nameof(TestAdd)}\\{nameof(InvalidCharacters)}";
            Directory.Exists(root).ShouldBeTrue();

            throw new NotImplementedException();
        }
    }
}