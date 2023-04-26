// See https://aka.ms/new-console-template for more information


using CommandLine;

public class Options
{
    [Option('a', "app", Required = true, HelpText = "The name of the application")]
    public string ApplicationName { get; set; }

    [Option('i', "image", Required = false, HelpText = "The base docker image to use for the database instance", Default = "postgres:15")]
    public string BaseDatabaseImage { get; set; }

    [Option('n', "name", Required = false, HelpText = "The name of the launched container", Default = null)]
    public string? InstanceName { get; set; }

    [Option('p', "port", Required = false, HelpText = "The public port for the database instance", Default = 5432)]
    public int PublicPort { get; set; }

    [Option('d', "database", Required = false, HelpText = "The name of the database (catalog) to use for data", Default = "data")]
    public string DbName {get; set; }

    [Option('u', "username", Required = false, HelpText = "The username for the admin access to the database instance.", Default = "postgres")]
    public string DbUsername { get; set; }

    [Option('p', "password", Required = false, HelpText = "The password for the admin access to the database instance.", Default = "Welcome@11")]
    public string DbPassword { get; set; }
}

[Verb("init", HelpText="Initialize a base database container if one doesn't exist.")]
public class InitOptions : Options
{
}


[Verb("up", HelpText = "Initialize a container if needed and run all missing migrations.")]
public class UpOptions : Options
{

    [Option('b', "base", Required = true, HelpText = "The folder to scan for migration files.")]
    public string BasePath { get; set; }
}
