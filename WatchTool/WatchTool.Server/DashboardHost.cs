using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace WatchTool.Server
{
    public class DashboardHost
    {
        private IWebHost webHost;

        public void Initialized(IEnumerable<ServiceDescriptor> services, IServiceProvider serviceProvider)
        {
            this.webHost = Dashboard.Program.CreateWebHost(services, serviceProvider, new WebHostBuilder());
        }

        // TODO dispose
    }
}
