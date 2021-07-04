using System;
using System.CommandLine;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using DotNetCli.CommandLineParser.Shared;
using System.CommandLine.Builder;
using System.Threading.Tasks;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Linq;
using Microsoft.Extensions.Options;

namespace DotNetCli.CommandLineParser
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var services = CreateServices();

            RootCommand rootCommand = new RootCommand(description: "Execute inventory items migrations.") { Name = "sql-migrator" };

            rootCommand.AddCommand(new UpCommand("up", services));

            rootCommand.AddGlobalOption(
                new Option<Uri>(new[] { "--h", "-h", "--consulHost" }, getDefaultValue: () => new Uri("http://localhost:8500"), description: "Define consul host endpoint")
                {
                    IsRequired = true
                });
            rootCommand.AddGlobalOption(
                new Option<string>(new[] { "--d", "-d", "--consulDatacenter" }, getDefaultValue: () => "default", description: "Define consul datacenter endpoint")
                {
                    IsRequired = true
                });

            return await new CommandLineBuilder(rootCommand)
                .Build()
                .InvokeAsync(args);
        }

        static IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();

            services
                .AddFluentMigratorCore()
                .ConfigureRunner(builder => builder
                    .AddSqlServer()
                    .WithGlobalConnectionString(sp =>
                    {
                        var commandLineArguments = sp.GetRequiredService<IOptionsMonitor<CommandLineArguments>>().CurrentValue;

                        return sp.GetRequiredService<ConsulService>().GetConnectionString(commandLineArguments);
                    })
                    .ScanIn(typeof(CommandLineArguments).Assembly).For.All());

            services.AddOptions<CommandLineArguments>();

            services.AddLogging(log => log.AddFluentMigratorConsole())
                .Configure<FluentMigratorLoggerOptions>(config =>
                {
                    config.ShowSql = true;
                    config.ShowElapsedTime = true;
                });

            return services.BuildServiceProvider();
        }
    }
}
