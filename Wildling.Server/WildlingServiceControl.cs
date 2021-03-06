﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.Web.Http;
using System.Web.Http.SelfHost;
using Autofac;
using Autofac.Integration.WebApi;
using Common.Logging;
using Newtonsoft.Json;
using Topshelf;
using Wildling.Core;
using Wildling.Core.Converters;

namespace Wildling.Server
{
    class WildlingServiceControl : ServiceControl
    {
        static readonly ILog Log = LogManager.GetLogger<WildlingServiceControl>();
        readonly CommandLineOptions _options;
        HttpSelfHostServer _server;

        public WildlingServiceControl(CommandLineOptions options)
        {
            _options = options;
        }

        public bool Start(HostControl hostControl)
        {
            var builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterInstance(_options);
            builder.RegisterType<Configuration>().SingleInstance();

            Node node = new Node(_options.Name, new[] { "A", "B", "C" });
            builder.RegisterInstance(node);

            var container = builder.Build();

            Uri address = node.GetUriBuilder(_options.Name).Uri;
            Log.TraceFormat("Self-hosted WebAPI hosted @ {0}", address);

            var config = new HttpSelfHostConfiguration(address);
            config.EnableCors();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional});
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            config.EnableSystemDiagnosticsTracing();

            // TODO: Use MEF to load custom JSON converters?
            // Add custom JSON converters...
            IList<JsonConverter> converters = config.Formatters.JsonFormatter.SerializerSettings.Converters;
            converters.Add(new DottedVersionVectorJsonConverter());
            converters.Add(new VersionedObjectJsonConverter());

            _server = new HttpSelfHostServer(config);
            try
            {
                _server.OpenAsync().Wait();
            }
            catch (AggregateException e)
            {
                if (e.GetBaseException() is AddressAccessDeniedException addressAccessDenied)
                {
                    // If you get an AddressAccessDeniedException you will need permissions (from admin prompt)
                    // netsh http add urlacl url=http://+:8080/ user=DOMAIN\USER
                    // See http://blogs.msdn.com/b/amitlale/archive/2007/01/29/addressaccessdeniedexception-cause-and-solution.aspx

                    Log.Fatal("Error starting the http self-host server", addressAccessDenied);
                    Log.Info("Hint: Create URL reservation with the following command (from admin prompt):");
                    Log.InfoFormat(@"netsh http add urlacl url={2}://+:{3}/ user={0}\{1}",
                        Environment.UserDomainName, Environment.UserName,
                        config.BaseAddress.Scheme, config.BaseAddress.Port);
                }

                throw;
            }

            return true;
        }

        public bool Stop(HostControl hostControl)
        {
            _server.CloseAsync().Wait();
            _server.Dispose();
            return true;
        }
    }
}