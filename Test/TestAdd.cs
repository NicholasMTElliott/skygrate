using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Skygrate.DatabaseProvider.Postgresql;
using Skyward.Skygrate.Abstractions;
using Skyward.Skygrate.Core;
using Skyward.Skygrate.MigrationProvider.LocalFileSystem;

namespace Test
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
        readonly static LaunchOptions Options = new LaunchOptions {
        };

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
                new LocalFileSystemMigrationProvider(loggerFactory.CreateLogger<LocalFileSystemMigrationProvider>(), new OptionsWrapper<Config>(new Config {
                    BasePath = root
                })),
                loggerFactory.CreateLogger<MigrationLogic>()
                );


            MigrationReference migration = await logic.AddNewMigrationAsync(nameof(NoPriorMigrations));
            migration.Id.ShouldNotBeNullOrWhiteSpace();
            migration.Id.Length.ShouldBe(8);
            migration.Timestamp.ShouldNotBeNullOrWhiteSpace();
            migration.Timestamp.Length.ShouldBe(14);
            migration.Checksum.ShouldBeNull();
            migration.Name.ShouldBe(nameof(NoPriorMigrations));
            migration.PriorId.ShouldBeNull();

            File.Exists($"{root}\\{migration.Timestamp}_{migration.Id}_{migration.Name}.sql").ShouldBeTrue();
        }

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


            MigrationReference migration = await logic.AddNewMigrationAsync(nameof(NoPriorMigrations));
            migration.Id.ShouldNotBeNullOrWhiteSpace();
            migration.Id.Length.ShouldBe(8);
            migration.Timestamp.ShouldNotBeNullOrWhiteSpace();
            migration.Timestamp.Length.ShouldBe(14);
            migration.Checksum.ShouldBeNull();
            migration.Name.ShouldBe(nameof(NoPriorMigrations));
            migration.PriorId.ShouldBe("12345678");

            File.Exists($"{root}\\{migration.Timestamp}_{migration.Id}_{migration.Name}.sql").ShouldBeTrue();
        }

        [TestMethod]
        public void DuplicateNameSpecified()
        {
            var root = $".\\{nameof(TestAdd)}\\{nameof(DuplicateNameSpecified)}";
            Directory.Exists(root).ShouldBeTrue();
            throw new NotImplementedException();
        }

        [TestMethod]
        public void InvalidCharacters()
        {
            var root = $".\\{nameof(TestAdd)}\\{nameof(InvalidCharacters)}";
            Directory.Exists(root).ShouldBeTrue();

            throw new NotImplementedException();
        }
    }
}