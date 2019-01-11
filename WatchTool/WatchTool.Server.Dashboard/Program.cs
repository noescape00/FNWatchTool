using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WatchTool.Server.Dashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }

        public static IWebHost CreateWebHost(IEnumerable<ServiceDescriptor> services, IServiceProvider serviceProvider, IWebHostBuilder webHostBuilder)
        {
            IWebHost host = webHostBuilder
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .ConfigureServices(collection =>
                {
                    if (services == null)
                        return;

                    // copies all the services
                    // also copies over singleton instances already defined
                    foreach (ServiceDescriptor service in services)
                    {
                        object obj = serviceProvider.GetService(service.ServiceType);

                        if (obj != null && service.Lifetime == ServiceLifetime.Singleton && service.ImplementationInstance == null)
                            collection.AddSingleton(service.ServiceType, obj);
                        else
                            collection.Add(service);
                    }
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();

            return host;
        }
    }
}
