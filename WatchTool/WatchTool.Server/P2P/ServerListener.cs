using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WatchTool.Common.P2P;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Server.P2P
{
    public class ServerListener : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>TCP server listener accepting inbound connections.</summary>
        private readonly TcpListener tcpListener;

        private readonly CancellationTokenSource cancellation;

        /// <summary>Task accepting new clients in a loop.</summary>
        private Task acceptTask;

        private readonly PayloadProvider payloadProvider;

        private readonly ServerConnectionManager connectionManager;

        public ServerListener(PayloadProvider payloadProvider, ServerConnectionManager connectionManager, ServerConfiguration config)
        {
            this.payloadProvider = payloadProvider;
            this.connectionManager = connectionManager;
            this.cancellation = new CancellationTokenSource();

            this.tcpListener = new TcpListener(config.ListenEndPoint);
            this.tcpListener.Server.LingerState = new LingerOption(true, 0);
            this.tcpListener.Server.NoDelay = true;
            this.tcpListener.Server.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

            this.acceptTask = Task.CompletedTask;

            this.logger.Debug("Server is ready to listen on '{0}'.", config.ListenEndPoint);
        }

        /// <summary>Starts listening on the server's endpoint.</summary>
        public void Initialize()
        {
            this.tcpListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this.tcpListener.Start();
            this.acceptTask = this.AcceptClientsAsync();
        }

        /// <summary>
        /// Implements loop accepting connections from newly connected clients.
        /// </summary>
        private async Task AcceptClientsAsync()
        {
            this.logger.Debug("Accepting incoming connections.");

            try
            {
                while (!this.cancellation.IsCancellationRequested)
                {
                    // Used to record any errors occurring in the thread pool task.
                    Exception error = null;

                    TcpClient tcpClient = await Task.Run(() =>
                    {
                        try
                        {
                            Task<TcpClient> acceptClientTask = this.tcpListener.AcceptTcpClientAsync();
                            acceptClientTask.Wait(this.cancellation.Token);
                            return acceptClientTask.Result;
                        }
                        catch (Exception exception)
                        {
                            // Record the error.
                            error = exception;
                            return null;
                        }
                    }).ConfigureAwait(false);

                    // Raise the error.
                    if (error != null)
                        throw error;

                    this.logger.Info("Connection established with {0}.", tcpClient.Client.RemoteEndPoint);
                    NetworkConnection connection = new NetworkConnection(tcpClient, payloadProvider);
                    ServerPeer peer = new ServerPeer(connection, this.connectionManager, failedPeer =>
                    {
                        this.connectionManager.RemovePeer(failedPeer as ServerPeer);
                    });

                    this.connectionManager.AddPeer(peer);
                }
            }
            catch (OperationCanceledException)
            {
                this.logger.Debug("Shutdown detected, stop accepting connections.");
            }
            catch (Exception e)
            {
                this.logger.Debug("Exception occurred: {0}", e.ToString());
            }
        }

        public void Dispose()
        {
            this.cancellation.Cancel();

            this.logger.Debug("Stopping TCP listener.");
            this.tcpListener.Stop();

            this.logger.Debug("Waiting for accepting task to complete.");
            this.acceptTask.Wait();

            this.cancellation.Dispose();
        }
    }
}
