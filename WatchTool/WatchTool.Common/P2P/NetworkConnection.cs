using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WatchTool.Common.P2P
{
    public class NetworkConnection : IDisposable
    {
        private readonly TcpClient client;

        /// <summary>Stream to send and receive messages through established TCP connection.</summary>
        private readonly NetworkStream stream;

        private readonly CancellationTokenSource cancellation;

        public NetworkConnection(TcpClient client)
        {
            this.client = client;
            this.stream = client.GetStream();

            this.cancellation = new CancellationTokenSource();
        }

        // TODO methods to send and receive things

        // Throws if there was a problem establishing connection
        public static async Task<NetworkConnection> EstablishConnection(IPEndPoint endPoint,
            CancellationToken cancellationToken)
        {
            var client = new TcpClient(AddressFamily.InterNetworkV6);
            client.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);

            // This variable records any error occurring in the thread pool task's context.
            Exception exception = null;

            await Task.Run(() =>
            {
                try
                {
                    client.ConnectAsync(endPoint.Address, endPoint.Port).Wait(cancellationToken);
                }
                catch (Exception e)
                {
                    // Record the error occurring in the thread pool's context.
                    exception = e;
                }
            }).ConfigureAwait(false);

            // Throw the error within this error handling context.
            if (exception != null)
                throw exception;

            var connection = new NetworkConnection(client);

            return connection;
        }

        // TODO too low level
        public async Task SendAsync(byte[] bytes)
        {
            await this.stream.WriteAsync(bytes, 0, bytes.Length, this.cancellation.Token).ConfigureAwait(false);
        }

        public void Dispose()
        {
            this.cancellation.Cancel();
        }
    }
}
