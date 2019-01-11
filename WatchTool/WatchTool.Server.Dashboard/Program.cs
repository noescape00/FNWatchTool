using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
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
            var dir = Directory.GetCurrentDirectory();


            IWebHost host = webHostBuilder
                .UseKestrel(options =>
                {
                    //Action<ListenOptions> configureListener = listenOptions => { listenOptions.UseHttps(certificate); };
                    //var ipAddresses = Dns.GetHostAddresses(apiSettings.ApiUri.DnsSafeHost);
                    //foreach (var ipAddress in ipAddresses)
                    //{
                    //    options.Listen(ipAddress, apiSettings.ApiPort, configureListener);
                    //}
                })
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
                //.UseUrls(apiUri.ToString()) // TODO
                .Build();

            host.Start();

            return host;
        }
    }
}
