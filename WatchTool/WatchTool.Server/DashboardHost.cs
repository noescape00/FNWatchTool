using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WatchTool.Server
{
    public class DashboardHost : IDisposable
    {
        private IWebHost webHost;

        public void Initialized(IEnumerable<ServiceDescriptor> services, IServiceProvider serviceProvider)
        {
            this.webHost = Dashboard.Program.CreateWebHost(services, serviceProvider, new WebHostBuilder());
        }

        public void Dispose()
        {
            this.webHost.Dispose();
        }
    }
}
