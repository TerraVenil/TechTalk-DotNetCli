namespace DotNetCli.CommandLineParser.Shared
{
    public interface IConsulService
    {
        string GetConnectionString(CommandLineArguments commandLineArguments);
    }

    public class ConsulService : IConsulService
    {
        public string GetConnectionString(CommandLineArguments commandLineArguments) => $"Host: {commandLineArguments.СonsulHost}, Node: {commandLineArguments.ConsulDatacenter}";
    }
}
