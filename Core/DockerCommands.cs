using Docker.DotNet;
using Docker.DotNet.Models;
using Newtonsoft.Json;
using Skyward.Skygrate.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Skyward.Skygrate.Core
{
    public class DockerCommands
    {
        public DockerCommands(LaunchOptions options) {
            this._options = options ?? throw new ArgumentNullException(nameof(options));
        }

        DockerClient client = new DockerClientConfiguration()
            .CreateClient();

        public async Task<IEnumerable<ImageReference>> QuerySnapshots()
        {
            var images = await client.Images.ListImagesAsync(new Docker.DotNet.Models.ImagesListParameters
            {
                All = true,
                Digests = true
            });

            var snapshotImages = images
                .Where(image => image.Labels != null && image.Labels.ContainsKey("_proc") && image.Labels["_proc"] == InternalSystemName)
                .Select(image => new ImageReference(
                    image,
                    image.Labels["_proc"],
                    image.Labels["param_check"],
                    image.Labels["application"],
                    image.Labels["rolling_checksum"],
                    image.Labels["prior_checksum"],
                    image.Labels["migration_id"],
                    image.Labels["named"]
                )).ToList();
            return snapshotImages;
        }

        public async Task RemoveSnapshotAsync(string iD)
        {
            await client.Images.DeleteImageAsync(iD, new ImageDeleteParameters {
            });
        }

        public async Task<IEnumerable<ContainerReference>> QueryContainers()
        {
            var containers = await client.Containers.ListContainersAsync(new ContainersListParameters
            {
                All = true
            });
            var skygrateContainers = containers
                .Where(container => container.Labels.ContainsKey("_proc") && container.Labels["_proc"] == InternalSystemName)
                .Select(container => new ContainerReference( 
                    container,
                    container.Labels["_proc"],
                    container.Labels["param_check"],
                    container.Labels["application"],
                    container.Labels["rolling_checksum"],
                    container.Labels["prior_checksum"],
                    container.Labels["migration_id"]
                )).ToList();
            return skygrateContainers;
        }



        static private readonly string InternalSystemName = Assembly.GetAssembly(typeof(DockerCommands))!.FullName!;
        private readonly LaunchOptions _options;

        /// <summary>
        /// Presumes there isn't one already launched
        /// </summary>
        public async Task<string> Launch()
        {

            var createResponse = await client.Containers.CreateContainerAsync(new Docker.DotNet.Models.CreateContainerParameters
            {
                Image = _options.BaseDatabaseImage, 
                Name = _options.InstanceName, 
                Labels = new Dictionary<string, string> { 
                    ["_proc"] = InternalSystemName,
                    ["param_check"] = _options.ParameterCheck, // checksum of important parameters
                    ["application"] = _options.ApplicationName,
                    ["rolling_checksum"] = "<INITIAL>",
                    ["prior_checksum"] = "<INITIAL>",
                    ["migration_id"] = "<INITIAL>",
                },
                Env = new List<string> {
                    $"POSTGRES_USER={_options.DbUsername}",
                    $"POSTGRES_DB={_options.DbName}",
                    $"POSTGRES_PASSWORD={_options.DbPassword}",
                },
                HostConfig = new HostConfig
                {
                    PortBindings = new Dictionary<string, IList<PortBinding>>
                    {
                        ["5432"] = new List<PortBinding> { 
                            new PortBinding { 
                                HostPort = "5455",//_options.PublicPort.ToString()
                                HostIP = "0.0.0.0",
                            }
                        }
                    }
                    
                },
                ExposedPorts = new Dictionary<string, EmptyStruct> 
                {
                    ["5432"] = new EmptyStruct() {  }
                }
            });

            var startResponse = await client.Containers.StartContainerAsync(createResponse.ID, new ContainerStartParameters {
            });

            return createResponse.ID;
        }

        public async Task TerminateContainer(string containerID)
        {
            await client.Containers.StopContainerAsync(containerID, new ContainerStopParameters { 
                WaitBeforeKillSeconds = 15,
            });

            await client.Containers.RemoveContainerAsync(containerID, new ContainerRemoveParameters { 
                Force = true
            });
        }

        public async Task SnapshotContainerAsync(string containerID, AppliedMigration? priorMigration, AppliedMigration appliedMigration, string runningChecksum, string? named = null)
        {
            await client.Containers.StopContainerAsync(containerID, new ContainerStopParameters
            {
                WaitBeforeKillSeconds = 60,
            });

            var commitResponse = await client.Images.CommitContainerChangesAsync(new CommitContainerChangesParameters
            {
                ContainerID = containerID,
                RepositoryName = _options.ApplicationName,
                Tag = named ?? $"{appliedMigration.Id}-{runningChecksum}",
                Config = new Config
                {
                    Labels = new Dictionary<string, string>
                    {
                        ["_proc"] = InternalSystemName,
                        ["param_check"] = _options.ParameterCheck, // checksum of important parameters
                        ["application"] = _options.ApplicationName,
                        ["rolling_checksum"] = runningChecksum,
                        ["prior_checksum"] = priorMigration.HasValue ? priorMigration.Value.Checksum : "<INITIAL>",
                        ["migration_id"] = appliedMigration.Id,
                        ["named"] = named ?? "" // Blank = automatic snapshot
                    },
                }
            });;
            var snapshotId = commitResponse.ID;

            await client.Images.TagImageAsync(snapshotId, new ImageTagParameters { 
                Tag = named ?? $"{appliedMigration.Id}-{runningChecksum}",
                Force = true,
                RepositoryName = _options.ApplicationName
            });

            await client.Containers.StartContainerAsync(containerID, new ContainerStartParameters
            {
            });
        }
    }

    public static class CollectionExtensions
    {
        public static TV GetValue<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defaultValue = default(TV))
        {
            TV value;
            return dict.TryGetValue(key, out value) ? value : defaultValue;
        }
    }

    public record ImageReference(ImagesListResponse Image, string System, string ParamCheck, string Application, string RollingChecksum, string PriorChecksum, string MigrationId, string Named);
    public record ContainerReference(ContainerListResponse Container, string System, string ParamCheck, string Application, string RollingChecksum, string PriorChecksum, string MigrationId);
}
