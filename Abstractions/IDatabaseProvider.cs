using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;

namespace Skyward.Skygrate.Abstractions
{
    public interface IDatabaseProvider
    {
        Task ApplyMigrationAsync(MigrationReference migration, string content);
        Task<bool> ConnectToDatabaseAsync(string dbUsername, string dbPassword, int publicPort, string dbName);
        Task<IList<AppliedMigration>> ListAppliedMigrationsAsync();
        Task<bool> ValidateInternalTablesAsync();
    }
}
