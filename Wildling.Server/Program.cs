using System;
using System.Reflection;
using System.Threading;
using Common.Logging;
using Topshelf;

namespace Wildling.Server
{
    class Program
    {
        static readonly ILog Log = LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {
            int exitCode = 0;
            Thread.CurrentThread.Name = "Main Thread";
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
            AdviseToAppDomainEvents();

            var options = new CommandLineOptions();
            try
            {
                HostFactory.Run(configurator =>
                {
                    options.ApplyCommandLine(configurator);

                    configurator.Service(() => new WildlingServiceControl(options));
                    configurator.SetServiceName("Wildling");
                    configurator.SetDisplayName("Wildling distributed key/value store");
                    configurator.SetDescription("A learning project for distributed data stores");

                    configurator.StartAutomaticallyDelayed();
                    configurator.RunAsNetworkService();
                });
            }
            catch (Exception e)
            {
                Log.Fatal("Error starting server: ", e);
                exitCode = 2;
            }

            return exitCode;
        }

        static void AdviseToAppDomainEvents()
        {
            Log.Debug("Subscribe to AppDomain events...");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
        }

        static Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs e)
        {
            if (!e.Name.Contains(".resources") && !e.Name.Contains(".XmlSerializers"))
            {
                Log.WarnFormat("AssemblyResolve: {0} RequestedBy: {1}", e.Name,
                    (e.RequestingAssembly != null) ? e.RequestingAssembly.FullName : null);
            }

            return null;
        }

        static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                Log.Error("Unhandled exception", exception);
            }
        }
    }
}
