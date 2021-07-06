using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using Microsoft.Extensions.Configuration;
using DotNetCli.CommandLineParser.Shared;
using Microsoft.Extensions.DependencyInjection;
using FluentMigrator.Runner;
using System;
using Microsoft.Extensions.Options;

namespace DotNetCli.CommandLineParser
{
    class Program
    {
        private static async Task<int> Main(string[] args)
        {
            RootCommand rootCommand = new RootCommand(description: "Execute inventory items migrations.") { Name = "sql-migrator" };

            rootCommand.AddCommand(new UpCommand("up"));

            rootCommand.AddGlobalOption(
                new Option<Uri>(new[] { "--h", "-h", "--consulHost", nameof(CommandLineArguments.СonsulHost) }, getDefaultValue: () => new Uri("http://localhost:8500"), description: "Define consul host endpoint")
                {
                    IsRequired = true
                });
            rootCommand.AddGlobalOption(
                new Option(new[] { "--d", "-d", "--consulDatacenter" }, description: "Define consul datacenter endpoint", typeof(string), getDefaultValue: () => "default", arity: ArgumentArity.ExactlyOne)
                {
                    IsRequired = true
                }.FromAmong("node1", "node2", "node3"));

            return await new CommandLineBuilder(rootCommand)
                .UseHost(host =>
                {
                    host
                        .ConfigureAppConfiguration((context, config) =>
                        {
                            config
                                .AddEnvironmentVariables();
                        })
                        .ConfigureServices((hostingContext, services) =>
                        {
                            services
                                .AddSingleton<IConsulService, ConsulService>()
                                .AddFluentMigratorCore()
                                .ConfigureRunner(builder => builder
                                    .AddSqlServer()
                                    .WithGlobalConnectionString(sp =>
                                    {
                                        var commandLineArguments = sp.GetRequiredService<IOptions<CommandLineArguments>>().Value;

                                        return sp.GetRequiredService<IConsulService>().GetConnectionString(commandLineArguments);
                                    })
                                    .ScanIn(typeof(CommandLineArguments).Assembly).For.All())
                                .AddLogging(log => log.AddFluentMigratorConsole())
                                .Configure<FluentMigratorLoggerOptions>(config =>
                                {
                                    config.ShowSql = true;
                                    config.ShowElapsedTime = true;
                                })
                                .Configure<InvocationLifetimeOptions>(options => options.SuppressStatusMessages = true)
                                .AddOptions<CommandLineArguments>()
                                .BindCommandLine();
                        });
                })
                .UseDefaults()
                .Build()
                .InvokeAsync(args);
        }
    }
}
