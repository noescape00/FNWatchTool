using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using WatchTool.Client.NodeIntegration;
using WatchTool.Client.P2P;
using WatchTool.Common;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Client
{
    public class ClientApplication : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IServiceProvider services;

        public async Task StartAsync(string[] args)
        {
            this.logger.Info("===CLIENT application starting===");

            try
            {
                this.services = this.GetServicesCollection().BuildServiceProvider();

                this.services.GetRequiredService<ClientConfiguration>().Initialize(new TextFileConfiguration(args));

                this.services.GetRequiredService<PayloadProvider>().DiscoverPayloads();
                this.services.GetRequiredService<ClientConnectionManager>().Initialize();
            }
            catch (Exception exception)
            {
                this.logger.Fatal(exception.ToString());
                throw;
            }

            this.logger.Trace("(-)");
        }

        protected virtual IServiceCollection GetServicesCollection()
        {
            this.logger.Trace("()");

            IServiceCollection collection = new ServiceCollection()
                .AddSingleton<ClientConnectionManager>()
                .AddSingleton<PayloadProvider>()
                .AddSingleton<NodeController>()
                .AddSingleton<ClientConfiguration>();

            this.logger.Trace("(-)");
            return collection;
        }

        public void Dispose()
        {
            this.logger.Trace("()");
            this.logger.Info("Application is shutting down...");

            this.services.GetRequiredService<ClientConnectionManager>()?.Dispose();

            this.logger.Info("Shutdown completed.");
            this.logger.Trace("(-)");
        }
    }
}
