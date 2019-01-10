using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace WatchTool.Common.P2P
{
    public class NetworkConnection : IDisposable
    {
        public static byte[] MagicBytes = { 0x73, 0x35, 0x22, 0x05 };

        public static int CommandNameSizeBytes = 20;

        public static int MessageLengthSize = 4;

        public static int MaxPayloadSize = 1024 * 32;

        private readonly TcpClient client;

        /// <summary>Stream to send and receive messages through established TCP connection.</summary>
        private readonly NetworkStream stream;

        private readonly CancellationTokenSource cancellation;

        private readonly Task receiveMessagesTask;

        //private readonly AsyncQueue<> receivedMessagesQueue;

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public NetworkConnection(TcpClient client)
        {
            this.client = client;
            this.stream = client.GetStream();

            //this.receivedMessagesQueue = new AsyncQueue<>();

            this.cancellation = new CancellationTokenSource();
            this.receiveMessagesTask = this.ReceiveMessagesAsync();
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
            // TODO remove
            string by = "";
            foreach (var b in bytes)
            {
                by += b.ToString() + " ";
            }
            this.logger.Info(by);

            await this.stream.WriteAsync(bytes, 0, bytes.Length, this.cancellation.Token).ConfigureAwait(false);
        }

        public async Task SendTestAsync()
        {
            List<byte> bytesToSend = new List<byte>();

            bytesToSend.AddRange(MagicBytes);

            for (int i = 0; i < CommandNameSizeBytes; ++i)
            {
                bytesToSend.Add(2);
            }

            uint length = 100;

            bytesToSend.AddRange(BitConverter.GetBytes(length));

            for (int i = 0; i < length; ++i)
            {
                bytesToSend.Add(8);
            }

            await SendAsync(bytesToSend.ToArray()).ConfigureAwait(false);
        }

        // gives us message or exception. if exception we should close connection immeduiatelly
        //public Task<> ReceiveIncomingMessageAsync(CancellationToken cancellation = default(CancellationToken))
        //{
        //    return this.receivedMessagesQueue.DequeueAsync(cancellation);
        //}

        /// <summary>Reads messages from the connection stream and pushes them to the queue.</summary>
        private async Task ReceiveMessagesAsync()
        {
            try
            {
                while (!this.cancellation.Token.IsCancellationRequested)
                {
                    byte[] rawMessage = await this.ReadMessageAsync().ConfigureAwait(false);

                    this.logger.Info("Message of lenght {0} received!", rawMessage.Length);

                    // TODO log incoming bytes
                    // Rewrite completely
                    //Message message = this.ReadMessage(rawMessage);
                    //message.MessageSize = (uint)rawMessage.Length;
                    //
                    //this.logger.LogTrace("Received message: '{0}'", message);
                    //
                    //var receivedMessage = new IncomingMessage(message, message.MessageSize);
                    //
                    //this.receivedMessagesQueue.Enqueue(receivedMessage);
                }
            }
            catch (Exception ex)
            {
                this.logger.Trace("Exception occurred: '{0}'", ex.ToString());

                //this.receivedMessagesQueue.Enqueue(new IncomingMessage(ex));
            }
        }

        /// <summary>Reads raw message in binary form from the connection stream.</summary>
        /// <returns>Binary message received from the connected counter party.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled or the end of the stream was reached.</exception>
        /// <exception cref="ProtocolViolationException">Thrown if the incoming message is too big.</exception>
        private async Task<byte[]> ReadMessageAsync()
        {
            // First find and read the magic.
            await this.ReadMagicAsync().ConfigureAwait(false);

            // Then read the header, which is formed of command and length.
            int headerSize = CommandNameSizeBytes + MessageLengthSize;

            var messageHeader = new byte[headerSize];
            await this.ReadBytesAsync(messageHeader, 0, headerSize).ConfigureAwait(false);

            // Then extract the length, which is the message payload size.
            int lengthOffset = CommandNameSizeBytes;
            uint length = BitConverter.ToUInt32(messageHeader, lengthOffset);

            if (length > MaxPayloadSize)
                throw new ProtocolViolationException("Message payload too big");

            // Read the payload.
            int magicLength = MagicBytes.Length;
            var message = new byte[magicLength + headerSize + length];

            await this.ReadBytesAsync(message, magicLength + headerSize, (int)length).ConfigureAwait(false);

            // And copy the magic and the header to form a complete message.
            Array.Copy(MagicBytes, 0, message, 0, MagicBytes.Length);
            Array.Copy(messageHeader, 0, message, MagicBytes.Length, headerSize);

            return message;
        }

        /// <summary>Seeks and reads the magic value from the connection stream.</summary>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled or the end of the stream was reached.</exception>
        /// <remarks>
        /// Each network message starts with the magic value. If the connection stream is in unknown state,
        /// the next bytes to read might not be the magic. Therefore we read from the stream until we find the magic value.
        /// </remarks>
        private async Task ReadMagicAsync()
        {
            var bytes = new byte[1];
            for (int i = 0; i < MagicBytes.Length; i++)
            {
                byte expectedByte = MagicBytes[i];

                await this.ReadBytesAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

                byte receivedByte = bytes[0];
                if (expectedByte != receivedByte)
                {
                    // If we did not receive the next byte we expected
                    // we either received the first byte of the magic value
                    // or not. If yes, we set index to 0 here, which is then
                    // incremented in for loop to 1 and we thus continue
                    // with the second byte. Otherwise, we set index to -1
                    // here, which means that after the loop incrementation,
                    // we will start from first byte of magic.
                    i = receivedByte == MagicBytes[0] ? 0 : -1;
                }
            }
        }

        /// <summary>
        /// Reads a specific number of bytes from the connection stream into a buffer.
        /// </summary>
        /// <param name="buffer">Buffer to read incoming data to.</param>
        /// <param name="offset">Position in the buffer where to write the data.</param>
        /// <param name="bytesToRead">Number of bytes to read.</param>
        /// <returns>Binary data received from the connected counterparty.</returns>
        /// <exception cref="OperationCanceledException">Thrown if the operation was cancelled or the end of the stream was reached.</exception>
        private async Task ReadBytesAsync(byte[] buffer, int offset, int bytesToRead)
        {
            while (bytesToRead > 0)
            {
                int chunkSize = await this.stream.ReadAsync(buffer, offset, bytesToRead, this.cancellation.Token).ConfigureAwait(false);
                if (chunkSize == 0)
                {
                    this.logger.Trace("(-)[STREAM_END]");
                    throw new OperationCanceledException();
                }

                offset += chunkSize;
                bytesToRead -= chunkSize;
            }
        }

        public void Dispose()
        {
            this.cancellation.Cancel();

            this.stream.Dispose();
            this.client.Close();

            this.receiveMessagesTask.GetAwaiter().GetResult();
            //this.receivedMessagesQueue.Dispose();

            this.cancellation.Dispose();
        }
    }
}
