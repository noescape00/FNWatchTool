using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using WatchTool.Common.P2P.PayloadsBase;
using WatchTool.Server.P2P;

namespace WatchTool.Server
{
    public class ServerApplication : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IServiceProvider services;

        public async Task StartAsync()
        {
            this.logger.Info("Application starting.");

            try
            {
                this.services = this.GetServicesCollection().BuildServiceProvider();

                this.services.GetRequiredService<PayloadProvider>().DiscoverPayloads();
                this.services.GetRequiredService<ServerListener>().Initialize();
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
                .AddSingleton<ServerListener>()
                .AddSingleton<PayloadProvider>();

            this.logger.Trace("(-)");
            return collection;
        }

        public void Dispose()
        {
            this.logger.Trace("()");
            this.logger.Info("Application is shutting down...");

            this.services.GetRequiredService<ServerListener>()?.Dispose();

            this.logger.Info("Shutdown completed.");
            this.logger.Trace("(-)");
        }
    }
}
