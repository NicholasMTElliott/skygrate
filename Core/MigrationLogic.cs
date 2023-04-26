using Microsoft.Extensions.Logging;
using Skyward.Skygrate.Abstractions;
using System.Data.Common;
using System.Reflection;

namespace Skyward.Skygrate.Core
{
    public class MigrationLogic
    {
        private readonly LaunchOptions _launchOptions;
        private readonly IDatabaseProvider _databaseProvider;
        private readonly IMigrationProvider _migrationProvider;
        private readonly ILogger<MigrationLogic> _logger;

        public MigrationLogic(LaunchOptions launchOptions, IDatabaseProvider databaseProvider, IMigrationProvider migrationProvider, ILogger<MigrationLogic> logger)
        {
            _launchOptions = launchOptions ?? throw new ArgumentNullException(nameof(launchOptions));
            _databaseProvider = databaseProvider ?? throw new ArgumentNullException(nameof(databaseProvider));
            _migrationProvider = migrationProvider ?? throw new ArgumentNullException(nameof(migrationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Public Commands

        public async Task<bool> InitializeAsync() // true on success
        {
            var runningContainer = await EstablishContainerAsync();
            if (runningContainer == null)
            {
                return false;
            }

            var db = await EstablishDatabaseAsync();
            if (!db)
            {
                return false;
            }

            return true;
        }

        public async Task<bool> UpAsync()
        {
            var runningContainer = await EstablishContainerAsync();
            if (runningContainer == null)
            {
                return false;
            }

            var db = await EstablishDatabaseAsync();
            if (!db)
            {
                return false;
            }

            // find all migrations
            var availableMigrations = await _migrationProvider.ListAvailableMigrationsAsync();

            var appliedMigrations = await _databaseProvider.ListAppliedMigrationsAsync();

            // pair off any already applied
            var unappliedMigrations = availableMigrations
                .Where(avail => !appliedMigrations.Any(applied => applied.Id == avail.Id && applied.Timestamp == avail.Timestamp))
                .OrderBy(avail => avail.Timestamp);

            // run the rest
            foreach (var migration in unappliedMigrations)
            {
                try
                {
                    var content = await _migrationProvider.GetContentForMigrationAsync(migration);
                    await _databaseProvider.ApplyMigrationAsync(migration, content);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"There was an error executing the migration ({migration.Id}) {migration.Name}: {ex.Message}");
                    _logger.LogError(ex.ToString());
                    return false;
                }
            }

            var postAppliedMigrations = (await _databaseProvider.ListAppliedMigrationsAsync()).OrderBy(m => m.Timestamp).ThenBy(m => m.Id);
            var stillMissingMigrations = availableMigrations
                .Where(avail => !postAppliedMigrations.Any(applied => applied.Id == avail.Id && applied.Timestamp == avail.Timestamp));
            if (stillMissingMigrations.Any())
            {
                _logger.LogError($"Attempted to apply all migrations, but there were some still unapplied: {
                    String.Join(", ", stillMissingMigrations.Select(m => $"({m.Id}) {m.Name}"))
                }");
            }
            var runningChecksum = MD5.CreateMD5(String.Join(",", postAppliedMigrations.Select(m => m.Checksum)));

            // Snapshot!
            var commands = new DockerCommands(_launchOptions);
            await commands.SnapshotContainerAsync(runningContainer.Container.ID, appliedMigrations.LastOrDefault(), postAppliedMigrations.Last(), runningChecksum);

            return true;
        }

        #endregion


        public async Task<ContainerReference?> EstablishContainerAsync()
        {
            ContainerReference? runningContainer = null;
            try
            {
                _logger.LogInformation("Checking for running container...");
                var commands = new DockerCommands(_launchOptions);

                var containers = await commands.QueryContainers();
                var containersForThisApplication = containers.Where(c => c.Application == _launchOptions.ApplicationName);
                var containersMatchingParamCheck = containersForThisApplication.Where(c => c.ParamCheck == _launchOptions.ParameterCheck);

                // Various cases here
                // Multiple matching param check? We need to cull some.
                if (containersMatchingParamCheck.Count() > 1)
                {
                    _logger.LogWarning("Found multiple containers potentially matching.");
                    _logger.LogInformation("You may:");
                    _logger.LogInformation("  1. Terminate all and recreate.");
                    _logger.LogInformation("  2. Terminate all but one (randomly chosen).");
                    _logger.LogInformation("  3. Abort.");
                    var userInput = Console.ReadKey();
                    switch (userInput.KeyChar)
                    {
                        case '1':
                            _logger.LogInformation("Terminating all existing containers.");
                            // Do that
                            foreach (var container in containersMatchingParamCheck)
                            {
                                await commands.TerminateContainer(container.Container.ID);
                            }
                            var id = await commands.Launch();
                            containers = await commands.QueryContainers();
                            runningContainer = containers.Where(c => c.Application == _launchOptions.ApplicationName).Single();
                            _logger.LogInformation($"Launched new base container {id} as {runningContainer.Container.Names.First()}");
                            break;
                        case '2':
                            _logger.LogInformation("Terminating all but one container.");
                            // Do that
                            foreach (var container in containersMatchingParamCheck.Skip(1))
                            {
                                await commands.TerminateContainer(container.Container.ID);
                            }
                            containers = await commands.QueryContainers();
                            runningContainer = containers.Where(c => c.Application == _launchOptions.ApplicationName).Single();
                            _logger.LogInformation($"Using running container {runningContainer.Container.ID} as {runningContainer.Container.Names.First()}");
                            break;
                        case '3':
                        default:
                            _logger.LogWarning("Aborting.");
                            return null;
                    }

                }
                // One matching? We are good to go
                else if (containersMatchingParamCheck.Count() == 1)
                {
                    runningContainer = containersMatchingParamCheck.Single();
                    _logger.LogInformation($"Found a running container {runningContainer.Container.ID} as {runningContainer.Container.Names.First()}");
                }
                // None matching but there are application containers? Prompt how to fix.
                else if (containersForThisApplication.Count() > 0)
                {
                    _logger.LogWarning("Found application container(s) but the parameters have changed since it was launched.");
                    _logger.LogInformation("You may:");
                    _logger.LogInformation("  1. Terminate and recreate.");
                    _logger.LogInformation("  2. Force use of an existing container (This may cause issues).");
                    _logger.LogInformation("  3. Abort.");

                    var userInput = Console.ReadKey();
                    switch (userInput.KeyChar)
                    {
                        case '1':
                            _logger.LogInformation("Terminating all existing containers.");
                            // Do that
                            foreach (var container in containersForThisApplication)
                            {
                                await commands.TerminateContainer(container.Container.ID);
                            }
                            var id = await commands.Launch();
                            containers = await commands.QueryContainers();
                            runningContainer = containers.Where(c => c.Application == _launchOptions.ApplicationName).Single();
                            _logger.LogInformation($"Launched new base container {id} as {runningContainer.Container.Names.First()}");
                            break;
                        case '2':
                            runningContainer = containersForThisApplication.Single();
                            _logger.LogInformation($"Using running container {runningContainer.Container.ID} as {runningContainer.Container.Names.First()}");
                            break;
                        case '3':
                        default:
                            _logger.LogWarning("Aborting.");
                            return null;
                    };
                }
                // None matching period? Just launch
                else
                {
                    var id = await commands.Launch();
                    containers = await commands.QueryContainers();
                    runningContainer = containers.Where(c => c.Application == _launchOptions.ApplicationName).Single();
                    _logger.LogInformation($"Launched new base container {id} as {runningContainer.Container.Names.First()}");
                }
            }
            catch (System.TimeoutException)
            {
                _logger.LogError("Timed out connecting to docker.  Is the subsystem running?");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown trying to initialize our docker container.");
                _logger.LogError(ex.ToString());
                return null;
            }

            return runningContainer;
        }

        public async Task<bool> EstablishDatabaseAsync()
        {
            _logger.LogInformation("Connecting to database...");
            try
            {
                var db = await _databaseProvider.ConnectToDatabaseAsync(_launchOptions.DbUsername, _launchOptions.DbPassword, _launchOptions.PublicPort, _launchOptions.DbName);
                if (db == null)
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown trying to connect to our database instance.");
                _logger.LogError(ex.ToString());
                return false;
            }

            try
            {
                _logger.LogInformation("Validating internal migration table...");
                var validated = await _databaseProvider.ValidateInternalTablesAsync();
                if (!validated)
                {
                    _logger.LogError("Not validated, but no error was thrown.");
                    return false;
                }
                _logger.LogInformation("Validated.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown trying to validate our internal migration table.");
                _logger.LogError(ex.ToString());
                return false;
            }

            return true;
        }

        public async Task<MigrationReference> AddNewMigrationAsync(string name)
        {
            return await _migrationProvider.AddNewMigrationAsync(name);
        }
    }
}
