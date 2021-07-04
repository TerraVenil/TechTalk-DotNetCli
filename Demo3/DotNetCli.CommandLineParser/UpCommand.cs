using DotNetCli.CommandLineParser.Shared;
using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.CommandLine.Invocation;
using static System.Console;

namespace DotNetCli.CommandLineParser
{
    public class UpCommand : Command
    {
        public UpCommand(string name) : base(name, "Executes up migration")
        {
            Handler = CommandHandler.Create<CommandLineArguments, IHost>(UpHanlder);
        }

        private void UpHanlder(CommandLineArguments commandLineArguments, IHost host)
        {
            WriteLine($"Execution of {nameof(UpHanlder)}");

            var connectionStringFromConsul = host.Services
                .GetRequiredService<IConsulService>()
                .GetConnectionString(commandLineArguments);

#if DEBUG
            WriteLine($"Migrating database on {connectionStringFromConsul}");
#endif

            using (var scope = host.Services.CreateScope())
            {
                var migrator = host.Services.GetService<IMigrationRunner>();
                //migrator.MigrateUp();
            }

            WriteLine("Database was migrated");
        }
    }
}
