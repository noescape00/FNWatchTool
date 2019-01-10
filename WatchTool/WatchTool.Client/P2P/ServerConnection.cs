using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WatchTool.Client.P2P
{
    public class ServerConnection
    {
        private readonly TcpClient client;

        private ServerConnection(TcpClient client)
        {
            this.client = client;
        }

        // TODO send and receive things

        // Throws if there was a problem establishing connection
        public static async Task<ServerConnection> EstablishConnection(IPEndPoint endPoint, CancellationToken cancellationToken)
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

            var connection = new ServerConnection(client);

            return connection;
        }
    }
}
