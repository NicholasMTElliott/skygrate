using Microsoft.Extensions.Logging;
using Skyward.Skygrate.Abstractions;
using System.ComponentModel;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        public async Task<Continuation<bool>> InitializeAsync() // true on success
        {
            var result = await EstablishContainerAsync();
            return await Resolve.After(result, async (container) =>
            {
                var db = await EstablishDatabaseAsync();
                if (!db)
                {
                    return Resolve.With(false);
                }

                return Resolve.With(true);
            });

        }

        public async Task<Continuation<bool>> UpAsync()
        {
            var establishContinuation = await EstablishContainerAsync();
            return await Resolve.After(establishContinuation, async (runningContainer) => { 
                if(runningContainer == null) 
                { 
                    return Resolve.With(false);
                }

                var db = await EstablishDatabaseAsync();
                if (!db)
                {
                    return Resolve.With(false);
                }

                // find all migrations
                var availableMigrations = await _migrationProvider.ListAvailableMigrationsAsync();
                var appliedMigrations = await _databaseProvider.ListAppliedMigrationsAsync();

                // For now just strip off any uncommited migrations
                var availableCommittedMigrations = availableMigrations.Where(m => m.Checksum != null);

                // pair off any already applied
                var unappliedMigrations = availableCommittedMigrations
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
                        return Resolve.With(false);
                    }
                }

                var postAppliedMigrations = (await _databaseProvider.ListAppliedMigrationsAsync()).OrderBy(m => m.Timestamp).ThenBy(m => m.Id);
                var stillMissingMigrations = availableCommittedMigrations
                    .Where(avail => !postAppliedMigrations.Any(applied => applied.Id == avail.Id && applied.Timestamp == avail.Timestamp));
                if (stillMissingMigrations.Any())
                {
                    _logger.LogError($"Attempted to apply all migrations, but there were some still unapplied: {String.Join(", ", stillMissingMigrations.Select(m => $"({m.Id}) {m.Name}"))}");
                }
                var runningChecksum = MD5.CreateMD5(String.Join(",", postAppliedMigrations.Select(m => m.Checksum)));

                // Snapshot!
                if (unappliedMigrations.Any())
                {
                    var commands = new DockerCommands(_launchOptions);
                    await commands.SnapshotContainerAsync(runningContainer.Container.ID, appliedMigrations.LastOrDefault(), postAppliedMigrations.Last(), runningChecksum);
                }

                return Resolve.With(true);
            });
        }

        #endregion


        public async Task<Continuation<ContainerReference?>> EstablishContainerAsync()
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
                    return new Continuation<ContainerReference?>(
                        "AmbiguousName",
                        "Found multiple containers potentially matching.",
                        false,
                        null,
                        new List<Option<ContainerReference?>>
                        {
                            new Option<ContainerReference?>(
                                "Recreate",
                                "Terminate all and recreate.",
                                async () => {
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
                                        return Resolve.With(runningContainer);
                                }
                            ),
                            new Option<ContainerReference?>(
                                "Reduce",
                                "Terminate all but one (randomly chosen)",
                                async () => {
                                    _logger.LogInformation("Terminating all but one container.");
                                    // Do that
                                    foreach (var container in containersMatchingParamCheck.Skip(1))
                                    {
                                        await commands.TerminateContainer(container.Container.ID);
                                    }
                                    containers = await commands.QueryContainers();
                                    runningContainer = containers.Where(c => c.Application == _launchOptions.ApplicationName).Single();
                                    _logger.LogInformation($"Using running container {runningContainer.Container.ID} as {runningContainer.Container.Names.First()}");
                                    return Resolve.With(runningContainer);
                                }
                            )
                        }
                    );
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
                    return new Continuation<ContainerReference?>(
                        "ParametersChanged",
                        "Found application container(s) but the parameters have changed since it was launched",
                        false, 
                        null,
                        new List<Option<ContainerReference?>>
                        {
                            new Option<ContainerReference?>(
                                "Recreate",
                                "Terminate and recreate.",
                                async () => {_logger.LogInformation("Terminating all existing containers.");
                                    // Do that
                                    foreach (var container in containersForThisApplication)
                                    {
                                        await commands.TerminateContainer(container.Container.ID);
                                    }
                                    var id = await commands.Launch();
                                    containers = await commands.QueryContainers();
                                    runningContainer = containers.Where(c => c.Application == _launchOptions.ApplicationName).Single();
                                    _logger.LogInformation($"Launched new base container {id} as {runningContainer.Container.Names.First()}");
                                    return Resolve.With(runningContainer);
                                }
                            ),
                            new Option<ContainerReference?>(
                                "Force",
                                "Force use of an existing container (This may cause issues)..",
                                async () => {
                                    runningContainer = containersForThisApplication.Single();
                                    _logger.LogInformation($"Using running container {runningContainer.Container.ID} as {runningContainer.Container.Names.First()}");
                                    return Resolve.With(runningContainer);
                                }
                            ),
                        }
                    );
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
                return Resolve.With<ContainerReference?>(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown trying to initialize our docker container.");
                _logger.LogError(ex.ToString());
                return Resolve.With<ContainerReference?>(null);
            }

            return Resolve.With(runningContainer);
        }

        public async Task<bool> EstablishDatabaseAsync()
        {
            _logger.LogInformation("Connecting to database...");
            try
            {
                var db = await _databaseProvider.ConnectToDatabaseAsync(_launchOptions.DbUsername, _launchOptions.DbPassword, _launchOptions.PublicPort, _launchOptions.DbName);
                if (!db)
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

        public async Task<Continuation<MigrationReference?>> AddNewMigrationAsync(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            var existingMigrations = await _migrationProvider.ListAvailableMigrationsAsync();
            if (existingMigrations.Any(m => m.Name.ToLower() == name.ToLower()))
            {
                return new Continuation<MigrationReference?>(
                    "DuplicateName",
                    "That name already exists as a migration, are you sure?",
                    false,
                    null,
                    new List<Option<MigrationReference?>>{ 
                        new Option<MigrationReference?>(
                            "Duplicate",
                            "Create a new migration with the same name",
                            async () => Resolve.With((MigrationReference?)await _migrationProvider.AddNewMigrationAsync(name))
                        )
                    }
                );
            }
            return Resolve.With((MigrationReference?)await _migrationProvider.AddNewMigrationAsync(name));
        }


        public async Task<IEnumerable<MigrationReference>> CommitPendingMigrationsAsync()
        {
            var availableMigrations = await _migrationProvider.ListAvailableMigrationsAsync();
            var pendingMigrations = availableMigrations.Where(m => m.Checksum == null);
            var committedMigrations = await Task.WhenAll(pendingMigrations.Select(_migrationProvider.CommitMigrationAsync).ToArray());
            return committedMigrations;
        }


        public async Task TerminateAsync()
        {
            try
            {
                _logger.LogInformation("Checking for running containers...");
                var commands = new DockerCommands(_launchOptions);

                var containers = await commands.QueryContainers();
                var containersForThisApplication = containers.Where(c => c.Application == _launchOptions.ApplicationName);
                _logger.LogInformation($"Found {containersForThisApplication.Count()} containers.");
                foreach (var container in containersForThisApplication)
                {
                    _logger.LogInformation($"Terminating container {container.Container.ID}...");
                    await commands.TerminateContainer(container.Container.ID);
                    _logger.LogInformation("Terminated.");
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
            }
        }

        public async Task SnapshotAsync(string name)
        {
            try
            {
                var commands = new DockerCommands(_launchOptions);
                throw new NotImplementedException();
                //await commands.SnapshotContainerAsync();
            }
            catch (System.TimeoutException)
            {
                _logger.LogError("Timed out connecting to docker.  Is the subsystem running?");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown trying to create a snapshot.");
                _logger.LogError(ex.ToString());
            }
        }

        /// <summary>
        /// Delete any unused snapshots.  By default only prunes automatic snapshots that don't match the migration chain.
        /// </summary>
        /// <param name="all">Remove all automatic snapshots, even if valid per the current migration chain.</param>
        /// <param name="named">Remove named snapshots as well.</param>
        public async Task PruneAsync(bool all, bool named)
        {
            try
            {
                _logger.LogInformation("Querying for snapshots...");
                var commands = new DockerCommands(_launchOptions);

                var snapshots = await commands.QuerySnapshots();
                var snapshotsToPrune = snapshots.Where(c => c.Application == _launchOptions.ApplicationName);
                if (!all)
                {
                    // More logic here to validate against the migration chain
                    throw new NotImplementedException("Currently you must prune all snapshots");
                }
                else
                {
                    _logger.LogInformation("Including valid snapshots.");
                }

                if (!named)
                {
                    snapshotsToPrune = snapshotsToPrune.Where(s => String.IsNullOrWhiteSpace(s.Named));
                }
                else
                {
                    _logger.LogInformation("Including named snapshots.");
                }
                _logger.LogInformation($"Found {snapshotsToPrune.Count()} snapshots to prune.");

                foreach (var snapshot in snapshotsToPrune)
                {
                    _logger.LogInformation($"Pruning snapshot {snapshot.Image.ID}...");
                    await commands.RemoveSnapshotAsync(snapshot.Image.ID);
                    _logger.LogInformation($"Pruned.");
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
            }
        }
    }
}
