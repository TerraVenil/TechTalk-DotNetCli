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
                new Option(new[] { "--d", "-d", "--consulDatacenter" }, description: "Define consul datacenter endpoint", typeof(string), getDefaultValue: () => "default", arity: ArgumentArity.ExactlyOne)
                {
                    IsRequired = true
                }.FromAmong("node1", "node2", "node3"));

            return await new CommandLineBuilder(rootCommand)
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
        }

        static IServiceProvider CreateServices()
        {
            var services = new ServiceCollection();

            services
                .AddSingleton<IConsulService, ConsulService>()
                .AddFluentMigratorCore()
                .ConfigureRunner(builder => builder
                    .AddSqlServer()
                    .WithGlobalConnectionString(sp =>
                    {
                        var commandLineArguments = sp.GetRequiredService<IOptionsMonitor<CommandLineArguments>>().CurrentValue;

                        return sp.GetRequiredService<IConsulService>().GetConnectionString(commandLineArguments);
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
