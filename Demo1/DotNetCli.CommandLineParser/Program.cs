using System;
using System.Linq;
using CommandLine;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using static System.Console;

var arguments = Parse(args);

static CommandLineArguments Parse(string[] args)
{
    var result = new CommandLineArguments();

    Parser.Default
        .ParseArguments<CommandLineArguments>(args)
        .WithParsed(p => result = p)
        .WithNotParsed(errors =>
        {
            var message = string.Join(Environment.NewLine, errors.Select(e => e.ToString()));
            Console.Error.WriteLine(message);
            throw new Exception(message);
        });

    return result;
}

var connectionStringFromConsul = GetConnectionStringFromConsul(arguments);

static string GetConnectionStringFromConsul(CommandLineArguments commandLindeArguments) => "";

#if DEBUG
WriteLine($"Migrating database on {connectionStringFromConsul}");
#endif

var services = CreateServices(arguments);

using (var scope = services.CreateScope())
{
    var migrator = services.GetService<IMigrationRunner>();
    //migrator.MigrateUp();
}

WriteLine("Database was migrated");

static IServiceProvider CreateServices(CommandLineArguments commandLineArguments)
{
    var services = new ServiceCollection();

    services
        .AddFluentMigratorCore()
        .ConfigureRunner(builder => builder
            .AddSqlServer()
            .WithGlobalConnectionString(GetConnectionStringFromConsul(commandLineArguments))
            .ScanIn(typeof(CommandLineArguments).Assembly).For.All());

    services.AddLogging(log => log.AddFluentMigratorConsole())
        .Configure<FluentMigratorLoggerOptions>(config =>
        {
            config.ShowSql = true;
            config.ShowElapsedTime = true;
        });

    return services.BuildServiceProvider();
}

public class CommandLineArguments
{
    public CommandLineArguments()
    {
        ConsulHost = new Uri("http://localhost:8500");
        ConsulDatacenter = "default";
    }

    [Option('h', Required = false)]
    public Uri ConsulHost { get; private set; }

    [Option('d', Required = false)]
    public string ConsulDatacenter { get; private set; }
}