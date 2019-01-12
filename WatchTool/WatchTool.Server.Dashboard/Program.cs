using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WatchTool.Server.Dashboard
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel(options => { })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build()
                .Run();
        }

        public static IWebHost CreateWebHost(IEnumerable<ServiceDescriptor> services, IServiceProvider serviceProvider, IWebHostBuilder webHostBuilder)
        {
            string location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // TODO hacky hack
            var root = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(location).FullName).FullName).FullName).FullName;
            var actualPath = Path.Combine(root, "WatchTool.Server.Dashboard");

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
                .UseContentRoot(actualPath)
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
                .UseUrls("http://localhost:5000", "http://*:80")
                //.UseUrls(apiUri.ToString()) // TODO
                .Build();

            host.Start();

            return host;
        }
    }
}
