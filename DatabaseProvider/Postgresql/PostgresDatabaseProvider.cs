using Microsoft.Extensions.Logging;
using Skyward.Skygrate.Abstractions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Skygrate.DatabaseProvider.Postgresql
{
    public class PostgresDatabaseProvider : IDatabaseProvider
    {
        const string InternalTableName = "_sg_migration_tracking";
        private readonly ILogger<PostgresDatabaseProvider> _logger;
        private Npgsql.NpgsqlConnection? _connection = null;

        public PostgresDatabaseProvider(ILogger<PostgresDatabaseProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task ApplyMigrationAsync(MigrationReference migration, string content)
        {
            _logger.LogInformation($"Applying migration ({migration.Id}) {migration.Name}...");
            try
            {
                using (var transactionScope = await _connection.BeginTransactionAsync())
                {
                    var cmd = _connection.CreateCommand();
                    cmd.Transaction = transactionScope;
                    cmd.CommandText = content;
                    await cmd.ExecuteNonQueryAsync();

                    // Insert into the database!
                    var sql = $"INSERT INTO {InternalTableName}" +
                        $" (Id, Timestamp, Name, Checksum, PreviousMigrationId, RollingChecksum)" +
                        $" VALUES (@{nameof(MigrationReference.Id)}, @{nameof(MigrationReference.Timestamp)}, @{nameof(MigrationReference.Name)}, @{nameof(MigrationReference.Checksum)}," +
                        $" @{nameof(MigrationReference.Id)}, @{nameof(MigrationReference.Checksum)})";
                    await _connection.ExecuteAsync(sql, migration);
                    transactionScope.Commit();
                    _logger.LogInformation($"Successful.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An exception was thrown trying to apply the migration ({migration.Id}) {migration.Name}. The transaction has been aborted.");
                throw;
            }
        }

        public async Task<bool> ConnectToDatabaseAsync(string dbUsername, string dbPassword, int publicPort, string dbName)
        {
            string connectionString = $"User ID={dbUsername};Password={dbPassword};Host=localhost;Port={publicPort};Database={dbName};";
            _connection = new Npgsql.NpgsqlConnection(connectionString);

            var attempts = 20; // Try 20 times
            var delay = 500; // Half second each
            Exception? storedEx = null;
            var connected = false;
            do
            {
                await Task.Delay(delay);
                try
                {
                    storedEx = null;
                    await _connection.OpenAsync();
                    connected = true;
                    Console.WriteLine("Connected.");
                }
                catch (Exception thrownEx)
                {
                    storedEx = thrownEx;
                }
            } while (!connected && --attempts > 0);

            if (!connected && storedEx != null)
            {
                _connection = null;
                throw storedEx;
            }

            return true;
        }

        public async Task<IList<AppliedMigration>> ListAppliedMigrationsAsync()
        {
            return (await _connection.QueryAsync<AppliedMigration>($"SELECT * from {InternalTableName} ORDER BY Applied, Timestamp")).ToList();
        }

        public async Task<bool> ValidateInternalTablesAsync()
        {
            // Make sure our migration table exists     
            var cmd = _connection.CreateCommand();
            {
                var assembly = Assembly.GetAssembly(typeof(PostgresDatabaseProvider));
                using var stream = assembly.GetManifestResourceStream("Skyward.Skygrate.DatabaseProvider.Postgresql.Resources.CreateMigrationTable.sql");
                using var reader = new StreamReader(stream);
                cmd.CommandText = await reader.ReadToEndAsync();
            }
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
    }
}
