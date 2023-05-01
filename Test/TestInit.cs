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
    /// init
    ///     nothing
    ///     exists but stopped
    ///     exists
    /// </summary>
    [TestClass]
    public class TestInit
    {
        const string AppName = nameof(TestInit);
        readonly static LaunchOptions Options = new LaunchOptions
        {
            ApplicationName = AppName,
            DbName = "Name",
            DbPassword = "Password",
            DbUsername = nameof(Options),
            PublicPort = 5433,
            InstanceName = AppName,
            BaseDatabaseImage = "postgres:15"
        };

        readonly static LaunchOptions OtherOptions = new LaunchOptions
        {
            ApplicationName = AppName,
            DbName = "Name",
            DbPassword = "Password",
            PublicPort = 5434,
            InstanceName = AppName,
            DbUsername = nameof(OtherOptions),
            BaseDatabaseImage = "postgres:15"
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
        public async Task NothingExistsYet()
        {
            var dbProviderMockOne = new Mock<IDatabaseProvider>(MockBehavior.Strict);
            dbProviderMockOne.Setup(m => m.ConnectToDatabaseAsync(Options.DbUsername, Options.DbPassword, Options.PublicPort, Options.DbName)).Returns(Task.FromResult(true));
            dbProviderMockOne.Setup(m => m.ValidateInternalTablesAsync()).Returns(Task.FromResult(true));

            var loggerFactory = new NullLoggerFactory();
            var logic = new MigrationLogic(
                Options,
                dbProviderMockOne.Object,
                new LocalFileSystemMigrationProvider(loggerFactory.CreateLogger<LocalFileSystemMigrationProvider>(), new OptionsWrapper<Config>(new Config
                {
                    BasePath = "."
                })),
                loggerFactory.CreateLogger<MigrationLogic>()
                );

            var result = await logic.InitializeAsync();
            result.Resolved.ShouldBeTrue();
            result.Value.ShouldBeTrue();

            dbProviderMockOne.VerifyAll();
        }

        [TestMethod]
        public void DbExistsButIsStopped()
        {
            throw new NotImplementedException();
        }

        [TestMethod, TestCategory("Gold Path")]
        public async Task InstanceExists()
        {
            // Step one: Start an instance
            var dbProviderMockOne = new Mock<IDatabaseProvider>(MockBehavior.Strict);
            dbProviderMockOne.Setup(m => m.ConnectToDatabaseAsync(Options.DbUsername, Options.DbPassword, Options.PublicPort, Options.DbName)).Returns(Task.FromResult(true));
            dbProviderMockOne.Setup(m => m.ValidateInternalTablesAsync()).Returns(Task.FromResult(true));

            var loggerFactory = new NullLoggerFactory();
            var logic = new MigrationLogic(
                Options,
                dbProviderMockOne.Object,
                new LocalFileSystemMigrationProvider(loggerFactory.CreateLogger<LocalFileSystemMigrationProvider>(), new OptionsWrapper<Config>(new Config
                {
                    BasePath = "."
                })),
                loggerFactory.CreateLogger<MigrationLogic>()
                );

            var result = await logic.InitializeAsync();
            result.Resolved.ShouldBeTrue();
            result.Value.ShouldBeTrue();

            // Step two: 'Init' again and make sure it doesn't do anything!var dbProviderMockOne = new Mock<IDatabaseProvider>(MockBehavior.Strict);
            var dbProviderMockTwo = new Mock<IDatabaseProvider>(MockBehavior.Strict);
            dbProviderMockTwo.Setup(m => m.ConnectToDatabaseAsync(Options.DbUsername, Options.DbPassword, Options.PublicPort, Options.DbName)).Returns(Task.FromResult(true));
            dbProviderMockTwo.Setup(m => m.ValidateInternalTablesAsync()).Returns(Task.FromResult(true));

            var logicSecondPass = new MigrationLogic(
                Options,
                dbProviderMockTwo.Object,
                new LocalFileSystemMigrationProvider(loggerFactory.CreateLogger<LocalFileSystemMigrationProvider>(), new OptionsWrapper<Config>(new Config
                {
                    BasePath = "."
                })),
                loggerFactory.CreateLogger<MigrationLogic>()
                );

            var initResult = await logicSecondPass.InitializeAsync();
            initResult.Resolved.ShouldBeTrue();
            initResult.Value.ShouldBeTrue();

            // TODO: This doesn't actually verify the instance exists, is the same, didn't get recreated, etc.
        }

        [TestMethod, TestCategory("Gold Path")]
        public async Task InstanceExistWithDifferentParameters()
        {
            await TestUtil.Terminate(OtherOptions);

            // Step one: Start an instance
            var dbProviderMockOne = new Mock<IDatabaseProvider>(MockBehavior.Strict);
            dbProviderMockOne.Setup(m => m.ConnectToDatabaseAsync(Options.DbUsername, Options.DbPassword, Options.PublicPort, Options.DbName)).Returns(Task.FromResult(true));
            dbProviderMockOne.Setup(m => m.ValidateInternalTablesAsync()).Returns(Task.FromResult(true));

            var loggerFactory = new NullLoggerFactory();
            var logic = new MigrationLogic(
                Options,
                dbProviderMockOne.Object,
                new LocalFileSystemMigrationProvider(loggerFactory.CreateLogger<LocalFileSystemMigrationProvider>(), new OptionsWrapper<Config>(new Config
                {
                    BasePath = "."
                })),
                loggerFactory.CreateLogger<MigrationLogic>()
                );

            var result = await logic.InitializeAsync();
            result.Resolved.ShouldBeTrue();
            result.Value.ShouldBeTrue();


            // Step one: Start an instance
            var dbProviderMockTwo = new Mock<IDatabaseProvider>(MockBehavior.Strict);
            dbProviderMockTwo.Setup(m => m.ConnectToDatabaseAsync(OtherOptions.DbUsername, OtherOptions.DbPassword, OtherOptions.PublicPort, OtherOptions.DbName)).Returns(Task.FromResult(true));
            dbProviderMockTwo.Setup(m => m.ValidateInternalTablesAsync()).Returns(Task.FromResult(true));

            var logicDifferentParameters = new MigrationLogic(
                OtherOptions,
                dbProviderMockTwo.Object,
                new LocalFileSystemMigrationProvider(loggerFactory.CreateLogger<LocalFileSystemMigrationProvider>(), new OptionsWrapper<Config>(new Config
                {
                    BasePath = "."
                })),
                loggerFactory.CreateLogger<MigrationLogic>()
                );

            var initResult = await logicDifferentParameters.InitializeAsync();
            initResult.Resolved.ShouldBeFalse();
            initResult.Name.ShouldBe("ParametersChanged");
            var option = initResult.Options.FirstOrDefault(o => o.Name == "Recreate");
            option.ShouldNotBeNull();

            initResult = await option.Resolver();
            initResult.Resolved.ShouldBeTrue();
            initResult.Value.ShouldBeTrue();
        }
    }
}