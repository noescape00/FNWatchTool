﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WatchTool.Common.P2P;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Client.P2P
{
    /// <summary>Maintains connection to server. Tries to restart it if failed.</summary>
    public class ConnectionManager : IDisposable
    {
        public ClientPeer ActivePeer { get; private set; }

        private readonly CancellationTokenSource cancellation;
        private Task connectingTask;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly PayloadProvider payloadProvider;

        public ConnectionManager(PayloadProvider payloadProvider)
        {
            this.ActivePeer = null;
            this.payloadProvider = payloadProvider;

            this.cancellation = new CancellationTokenSource();
        }

        /// <summary>Starts attempts to establish a connection.</summary>
        public void Initialize()
        {
            this.connectingTask = this.MaintainConnectionAsync();
        }

        private async Task MaintainConnectionAsync()
        {
            while (!this.cancellation.IsCancellationRequested)
            {
                if (this.ActivePeer != null)
                {
                    await Task.Delay(500, this.cancellation.Token).ConfigureAwait(false);

                    continue;
                }

                try
                {
                    this.logger.Info("Connecting to server...");

                    NetworkConnection connection = await NetworkConnection.EstablishConnection(ClientConfiguration.ServerEndPoint, this.payloadProvider,
                        this.cancellation.Token).ConfigureAwait(false);

                    this.ActivePeer = new ClientPeer(connection, peer =>
                    {
                        this.logger.Warn("Connection with the server was terminated.");
                        this.ActivePeer = null;
                    });

                    this.logger.Info("Connection with the server was established.");
                }
                catch (Exception e)
                {
                    this.logger.Warn("Failed attempt to establish a connection to the server.");
                    this.logger.Debug("Exception while trying to establish a connection to the server: '{0}'", e.ToString());

                    await Task.Delay(2000, this.cancellation.Token).ConfigureAwait(false);
                }
            }
        }

        public void Dispose()
        {
            this.cancellation.Cancel();

            this.connectingTask.GetAwaiter().GetResult();
            this.cancellation.Dispose();
        }
    }
}
