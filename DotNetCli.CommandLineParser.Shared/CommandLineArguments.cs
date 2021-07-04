using System;

namespace DotNetCli.CommandLineParser.Shared
{
    public class CommandLineArguments
    {
        public CommandLineArguments()
        {
        }

        public CommandLineArguments(Uri consulHost, string consulDatacenter)
        {
            СonsulHost = consulHost;
            ConsulDatacenter = consulDatacenter;
        }

        public Uri СonsulHost { get; set; }

        public string ConsulDatacenter { get; set; }
    }
}
