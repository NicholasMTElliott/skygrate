using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyward.Skygrate.Abstractions
{
    public interface IMigrationProvider
    {
        Task<IList<MigrationReference>> ListAvailableMigrationsAsync();
        Task<string> GetContentForMigrationAsync(MigrationReference migration);
        Task<MigrationReference> AddNewMigrationAsync(string name);
        Task<MigrationReference> CommitMigrationAsync(MigrationReference m);
    }
}
