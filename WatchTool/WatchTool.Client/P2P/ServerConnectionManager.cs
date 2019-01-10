using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WatchTool.Common.P2P;

namespace WatchTool.Client.P2P
{
    /// <summary>Maintains connection to server. Tries to restart it if failed.</summary>
    public class ServerConnectionManager : IDisposable
    {
        private NetworkConnection activeConnection;

        private readonly CancellationTokenSource cancellation;
        private Task connectingTask;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public ServerConnectionManager()
        {
            this.activeConnection = null;

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
                if (this.activeConnection != null)
                {
                    await Task.Delay(500, this.cancellation.Token).ConfigureAwait(false);

                    continue;
                }

                try
                {
                    this.logger.Info("Connecting to server...");

                    this.activeConnection = await NetworkConnection.EstablishConnection(ClientConfiguration.ServerEndPoint, this.cancellation.Token).ConfigureAwait(false);

                    this.logger.Info("Connection established.");
                }
                catch (Exception e)
                {
                    this.logger.Warn("Exception while trying to establish a connection to the server: '{0}'", e.ToString());

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
