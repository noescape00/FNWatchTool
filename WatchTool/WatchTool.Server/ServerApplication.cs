﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using WatchTool.Common;
using WatchTool.Common.Models;
using WatchTool.Common.P2P.PayloadsBase;
using WatchTool.Server.P2P;

namespace WatchTool.Server
{
    public class ServerApplication : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected IServiceProvider services;

        public async Task StartAsync(string[] args)
        {
            this.logger.Info("===SERVER application starting===");

            try
            {
                IServiceCollection servicesCollection = this.GetServicesCollection();
                this.services = servicesCollection.BuildServiceProvider();

                this.services.GetRequiredService<ServerConfiguration>().Initialize(new TextFileConfiguration(args));

                this.services.GetRequiredService<PayloadProvider>().DiscoverPayloads();
                this.services.GetRequiredService<ServerListener>().Initialize();

                this.services.GetRequiredService<DashboardHost>().Initialized(servicesCollection, this.services);
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
                .AddSingleton<PayloadProvider>()
                .AddSingleton<ServerConnectionManager>()
                .AddSingleton(provider => provider.GetService<ServerConnectionManager>() as IPeersController)
                .AddSingleton<DashboardHost>()
                .AddSingleton<ServerConfiguration>();

            this.logger.Trace("(-)");
            return collection;
        }

        public void Dispose()
        {
            this.logger.Trace("()");
            this.logger.Info("Application is shutting down...");

            this.services.GetRequiredService<ServerListener>()?.Dispose();
            this.services.GetRequiredService<ServerConnectionManager>()?.Dispose();
            this.services.GetRequiredService<DashboardHost>()?.Dispose();

            this.logger.Info("Shutdown completed.");
            this.logger.Trace("(-)");
        }
    }
}
