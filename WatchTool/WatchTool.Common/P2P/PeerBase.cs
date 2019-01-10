using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Common.P2P
{
    // sends messages to attached connection and handles messages for it
    public abstract class PeerBase : IDisposable
    {
        private readonly NetworkConnection connection;

        private readonly Action<PeerBase> onDisconnectedAndDisposed;

        private readonly CancellationTokenSource cancellation;

        private Task consumeMessagesTask;

        protected readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected PeerBase(NetworkConnection connection, Action<PeerBase> onDisconnectedAndDisposed)
        {
            this.connection = connection;
            this.onDisconnectedAndDisposed = onDisconnectedAndDisposed;

            this.consumeMessagesTask = this.ConsumeMessagesContinouslyAsync();
            this.cancellation = new CancellationTokenSource();
        }

        public async Task SendAsync(Payload payload)
        {
            try
            {
                await this.connection.SendAsync(payload).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.logger.Trace("Failed to send a message. Exception: '{0}'.", e.ToString());

                Task.Run(() => this.Dispose());
            }
        }

        private async Task ConsumeMessagesContinouslyAsync()
        {
            try
            {
                while (!this.cancellation.IsCancellationRequested)
                {
                    Payload payload = await this.connection.ReceiveIncomingMessageAsync(this.cancellation.Token).ConfigureAwait(false);

                    if (payload == null)
                    {
                        Task.Run(() => this.Dispose());
                        return;
                    }

                    this.OnPayloadReceived(payload);
                }
            }
            catch (OperationCanceledException e)
            {
            }
        }

        protected abstract void OnPayloadReceived(Payload payload);

        public void Dispose()
        {
            this.cancellation.Cancel();

            this.consumeMessagesTask.GetAwaiter().GetResult();

            this.cancellation.Dispose();

            Task.Run(() => onDisconnectedAndDisposed);
        }
    }
}
