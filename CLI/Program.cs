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


Commands command = Commands.Up;

var parserResult = Parser.Default.ParseArguments<InitOptions, UpOptions>(args);
var options = parserResult
    .MapResult<InitOptions, UpOptions, Options?>(
        (initOptions) => { command = Commands.Init; return initOptions; },
        (upOptions) => { command = Commands.Up; return upOptions; },
        errs => null
); 


if (options == null)
{
    return -1;
}

using var host = Host.CreateDefaultBuilder(args)
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

                services.Configure<Config>(c => c.BasePath = (options as UpOptions)?.BasePath);
                services.AddScoped<IMigrationProvider, LocalFileSystemMigrationProvider>();
            })
            .Build();

var config = host.Services.GetRequiredService<IConfiguration>();


var logic = host.Services.GetRequiredService<MigrationLogic>();
switch(command)
{
    case Commands.Init:
        if (!await logic.InitializeAsync())
        {
            return -1;
        }
        break;
    case Commands.Up:
        if (!await logic.UpAsync())
        {
            return -1;
        }
        break;
}

return 0;


enum Commands
{
    Up = 0,
    Init = 1
};

