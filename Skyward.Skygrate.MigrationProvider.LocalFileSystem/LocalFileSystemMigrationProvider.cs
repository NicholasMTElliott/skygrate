using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Skyward.Skygrate.Abstractions;
using System.Text.RegularExpressions;
using System.Text;
using Skyward.Skygrate.Core;

namespace Skyward.Skygrate.MigrationProvider.LocalFileSystem
{
    public class Config
    {
        public string BasePath { get; set; }
    }

    public class LocalFileSystemMigrationProvider : IMigrationProvider
    {
        private readonly ILogger<LocalFileSystemMigrationProvider> _logger;
        private readonly Config _config;
        private const string SentinelId = "00000000";
        private const string SentinelChecksum = "00000000000000000000000000000000";
        private static readonly Regex filenameRegex = new Regex("([^_]{14})_([^_]{8})_(.+)\\.sql", RegexOptions.IgnoreCase);
        private static readonly Regex commentRefex = new Regex($"^-- {nameof(LocalFileSystemMigrationProvider)}:.* ID:([^_]{{8}}) PRIOR:([^_]{{8}}) CHECKSUM:([^_]{{32}})");

        public LocalFileSystemMigrationProvider(ILogger<LocalFileSystemMigrationProvider> logger, IOptions<Config> config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config.Value ?? throw new ArgumentNullException(nameof(config));
            if (_config.BasePath == null)
            {
                throw new ArgumentNullException(nameof(_config.BasePath));
            }

            if (!Directory.Exists(_config.BasePath))
            {
                throw new ArgumentException($"Base folder {_config.BasePath} does not appear to exist");
            }
        }


        public async Task<IList<MigrationReference>> ListAvailableMigrationsAsync()
        {
            var parsed = await EnumerateMigrationsAsync();
            return parsed.Select(file => file.migration).ToList();
        }

        private async Task<IEnumerable<(string path, MigrationReference migration)>> EnumerateMigrationsAsync()
        {
            var files = System.IO.Directory.EnumerateFiles(_config.BasePath, "*.sql", SearchOption.AllDirectories);
            return files
                .Select(file => new { path = file, file = new FileInfo(file) })
                .Select(file => (path: file.path, migration: Parse(file.file)))
                .Where(file => file.migration != null)
                .Select(file => (path: file.path, migration: (MigrationReference)file.migration!))
                .OrderBy(file => file.migration!.Timestamp).ThenBy(file => file.migration!.Id);
        }

        private MigrationReference? Parse(FileInfo file)
        {
            var filename = file.Name;
            var match = filenameRegex.Match(filename);
            if (!match.Success)
            {
                return null;
            }

            DateTimeOffset ordinal;
            if (!DateTimeOffset.TryParseExact(match.Groups[1].Value, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.AssumeUniversal, out ordinal))
            {
                throw new ArgumentOutOfRangeException($"Invalid timestamp {match.Groups[1].Value} from migration file {filename}");
            }

            const Int32 BufferSize = 4096;
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
            {
                var line = streamReader.ReadLine();
                if (line == null)
                {
                    //throw new ArgumentOutOfRangeException($"Empty read from migration file {filename}");
                    return null;
                }
                var commentMatch = commentRefex.Match(line);
                if (!commentMatch.Success)
                {
                    //throw new ArgumentOutOfRangeException($"Invalid format from migration file {filename} in first line.  You may need to rebuild the migration: {line}");
                    return null;
                }

                return new MigrationReference
                {
                    Timestamp = match.Groups[1].Value,
                    Id = match.Groups[2].Value,
                    Name = match.Groups[3].Value,
                    InternalId = commentMatch.Groups[1].Value,
                    Checksum = commentMatch.Groups[3].Value == SentinelChecksum ? null : commentMatch.Groups[3].Value,
                    PriorId = commentMatch.Groups[2].Value == SentinelId ? null : commentMatch.Groups[2].Value,
                };
            }
        }

        public async Task<MigrationReference> AddNewMigrationAsync(string name)
        {
            var existing = await ListAvailableMigrationsAsync();
            var newMigration = new MigrationReference {
                Checksum = null,
                PriorId = existing.Any() ? existing.Last().Id : null,
                Timestamp = DateTimeOffset.UtcNow.UtcDateTime.ToString("yyyyMMddHHmmss"),
                Name = name,
                Id = Guid.NewGuid().ToString().Substring(0, 8)
            };
            await File.WriteAllTextAsync(
                $"{_config.BasePath}\\{newMigration.Timestamp}_{newMigration.Id}_{newMigration.Name}.sql", 
                $"-- {nameof(LocalFileSystemMigrationProvider)}:{newMigration.Name} ID:{newMigration.Id} PRIOR:{newMigration.PriorId ?? SentinelId} CHECKSUM:{SentinelChecksum}\n\n-- Do not edit the lines above. Add your SQL below here.\n\n", 
                Encoding.UTF8);
            return newMigration;
        }

        public async Task<MigrationReference> CommitMigrationAsync(MigrationReference migration)
        {
            var parsed = await EnumerateMigrationsAsync();
            var match = parsed.Single(p => p.migration.Id == migration.Id
                && p.migration.Timestamp == migration.Timestamp
                && p.migration.Checksum == migration.Checksum);

            // Strip the first line
            var content = await File.ReadAllLinesAsync(match.path);
            var allButHeader = content.Skip(1);
            var checksum = MD5.CreateMD5(allButHeader, nameof(LocalFileSystemMigrationProvider));
            var committedPrefixLine = $"-- {nameof(LocalFileSystemMigrationProvider)}:{migration.Name} ID:{migration.Id} PRIOR:{migration.PriorId ?? SentinelId} CHECKSUM:{checksum}";
            var newlines = new List<string> { committedPrefixLine }.Union(allButHeader);
            await File.WriteAllLinesAsync(match.path, newlines, Encoding.UTF8 );
            return new MigrationReference {
                Id = migration.Id,
                Checksum = checksum,
                Name= migration.Name,
                PriorId= migration.PriorId,
                Timestamp = migration.Timestamp
            };

        }


        public async Task<string> GetContentForMigrationAsync(MigrationReference migration)
        {
            var parsed = await EnumerateMigrationsAsync();
            var match = parsed.Single(p => p.migration.Id == migration.Id
                && p.migration.Timestamp == migration.Timestamp
                && p.migration.Checksum == migration.Checksum);

            // Strip the first line
            var content = await File.ReadAllLinesAsync(match.path);
            return String.Join("\n", content.Skip(1));
        }

        public async Task<string> GetChecksumForMigrationAsync(MigrationReference migration)
        {
            // Find the matching migration
            var parsed = await EnumerateMigrationsAsync();
            var match = parsed.Single(p => p.migration.Id == migration.Id
                && p.migration.Timestamp == migration.Timestamp
                && p.migration.Checksum == migration.Checksum);

            // Strip the first line
            var content = await File.ReadAllLinesAsync(match.path);
            var allButHeader = content.Skip(1);
            var checksum = MD5.CreateMD5(allButHeader, nameof(LocalFileSystemMigrationProvider));
            return checksum;
        }
    }
}