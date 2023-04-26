using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;

namespace Skyward.Skygrate.Abstractions
{
    public struct AppliedMigration
    {
        public string Timestamp { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Checksum { get; set; }

        public string PreviousMigrationId { get; set; }
        public DateTimeOffset Applied { get; set; }
        public string RollingChecksum { get; set; }
    }

    public struct MigrationReference
    {
        public string Timestamp { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// May be null for a newly created migration
        /// </summary>
        public string Checksum { get; set; }

        /// <summary>
        /// May be null for the first-ever migration
        /// </summary>
        public string PriorId { get; set; }
    }
}