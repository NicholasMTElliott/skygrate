// See https://aka.ms/new-console-template for more information


using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Skygrate.DatabaseProvider.Postgresql;
using Skyward.Skygrate.Abstractions;
using Skyward.Skygrate.Core;
using Skyward.Skygrate.MigrationProvider.LocalFileSystem;

Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Warning()
            .WriteTo.Console()
            .CreateLogger();


var parserResult = Parser.Default.ParseArguments<InitOptions, UpOptions, PruneOptions, TerminateOptions>(args);
var actionResult = parserResult
    .MapResult<InitOptions, UpOptions, PruneOptions, TerminateOptions, Task<int>>(
        async (initOptions) => {
            IHost host = BuildHost(args, initOptions);
            var logic = host.Services.GetRequiredService<MigrationLogic>();
            var continuation = await logic.InitializeAsync();
            var result = await ResolveContinuations(continuation);
            if (!result)
            {
                return -1;
            }
            return 0;
        },
        async (upOptions) => {
            IHost host = BuildHost(args, upOptions);
            var logic = host.Services.GetRequiredService<MigrationLogic>();
            var continuation = await logic.UpAsync();
            var result = await ResolveContinuations(continuation);
            if (!result)
            {
                return -1;
            }
            return 0;
        },
        async (pruneOptions) => {
            IHost host = BuildHost(args, pruneOptions);
            var logic = host.Services.GetRequiredService<MigrationLogic>();
            await logic.PruneAsync(pruneOptions.All, pruneOptions.Named);
            return 0;
        },
        async (terminateOptions) =>
        {
            IHost host = BuildHost(args, terminateOptions);
            var logic = host.Services.GetRequiredService<MigrationLogic>();
            await logic.TerminateAsync();
            if (terminateOptions.Prune)
            {
                await logic.PruneAsync(true, true);
            }
            return 0;
        },
        errs => null
);
return await actionResult;


static IHost BuildHost(string[] args, Options? options)
{
    return Host.CreateDefaultBuilder(args)
                .UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration))
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(new LaunchOptions
                    {
                        ApplicationName = options.ApplicationName,
                        InstanceName = options.InstanceName ?? $"{options.ApplicationName}-db",
                        PublicPort = options.PublicPort,
                        BaseDatabaseImage = options.BaseDatabaseImage,
                        DbPassword = options.DbPassword,
                        DbName = options.DbName,
                        DbUsername = options.DbUsername
                    });

                    services.AddScoped<DockerCommands>();
                    services.AddScoped<MigrationLogic>();
                    services.AddScoped<IDatabaseProvider, PostgresDatabaseProvider>();

                    services.Configure<Config>(c => c.BasePath = (options as UpOptions)?.BasePath ?? ".");
                    services.AddScoped<IMigrationProvider, LocalFileSystemMigrationProvider>();
                })
                .Build();
}

static async Task<T> ResolveContinuations<T>(Continuation<T> continuation)
{
    while (!continuation.Resolved)
    {
        Console.WriteLine(continuation.Description);
        for (var i = 0; i < continuation.Options.Count; ++i)
        {
            Console.WriteLine($"  {i + 1}: {continuation.Options[i].Description}");
        }
        Console.WriteLine($" {continuation.Options.Count + 1}: Abort");
        Console.Write("> ");
        var choice = Console.ReadLine();
        int chosen;
        if (int.TryParse(choice, out chosen) && chosen > 0 && chosen < continuation.Options.Count + 1)
        {
            continuation = await continuation.Options[chosen - 1].Resolver();
        }
        else
        {
            return default(T);
        }
    }

    return continuation.Value;
}

