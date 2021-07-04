using DotNetCli.CommandLineParser.Shared;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using static System.Console;

namespace DotNetCli.CommandLineParser
{
    public class UpCommand : Command
    {
        private readonly IServiceProvider _serviceProvider;

        public UpCommand(string name, IServiceProvider serviceProvider) : base(name, "Executes up migration")
        {
            _serviceProvider = serviceProvider;
            Handler = CommandHandler.Create<CommandLineArguments, ConsulService>(UpHandler);
        }

        private void UpHandler(CommandLineArguments commandLineArguments, ConsulService consulService)
        {
            var commandLineArgumentsOptions = _serviceProvider.GetRequiredService<IOptionsMonitor<CommandLineArguments>>();

            commandLineArgumentsOptions.CurrentValue.СonsulHost = commandLineArguments.СonsulHost;
            commandLineArgumentsOptions.CurrentValue.ConsulDatacenter = commandLineArguments.ConsulDatacenter;

            WriteLine($"Execution of {nameof(UpHandler)}");

            var connectionStringFromConsul = consulService.GetConnectionString(commandLineArguments);

#if DEBUG
            WriteLine($"Migrating database on {connectionStringFromConsul}");
#endif

            using (var scope = _serviceProvider.CreateScope())
            {
                var migrator = _serviceProvider.GetService<IMigrationRunner>();
                //migrator.MigrateUp();
            }

            WriteLine("Database was migrated");
        }
    }
}
