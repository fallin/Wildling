using System;
using Topshelf.HostConfigurators;

namespace Wildling.Server
{
    class CommandLineOptions
    {
        public string Name { get; set; }

        public void ApplyCommandLine(HostConfigurator configurator)
        {
            configurator.AddCommandLineDefinition("name", v => Name = v);
            configurator.ApplyCommandLine();
        }
    }
}