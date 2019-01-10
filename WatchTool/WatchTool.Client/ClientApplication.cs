using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using WatchTool.Client.P2P;

namespace WatchTool.Client
{
    public class ClientApplication : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IServiceProvider services;

        public async Task StartAsync()
        {
            this.logger.Info("Application starting.");

            try
            {
                this.services = this.GetServicesCollection().BuildServiceProvider();

                this.services.GetRequiredService<ConnectionManager>().Initialize();
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
                .AddSingleton<ConnectionManager>();

            this.logger.Trace("(-)");
            return collection;
        }

        public void Dispose()
        {
            this.logger.Trace("()");
            this.logger.Info("Application is shutting down...");

            this.services.GetRequiredService<ConnectionManager>()?.Dispose();

            this.logger.Info("Shutdown completed.");
            this.logger.Trace("(-)");
        }
    }
}
