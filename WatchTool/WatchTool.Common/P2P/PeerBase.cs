using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Common.P2P
{
    // sends messages to attached connection and handles messages for it
    public abstract class PeerBase : IDisposable
    {
        public NetworkConnection Connection { get; private set; }

        private readonly Action<PeerBase> onDisconnectedAndDisposed;

        protected readonly CancellationTokenSource cancellation;

        private readonly Task consumeMessagesTask;

        protected readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected PeerBase(NetworkConnection connection, Action<PeerBase> onDisconnectedAndDisposed)
        {
            this.Connection = connection;
            this.onDisconnectedAndDisposed = onDisconnectedAndDisposed;

            this.cancellation = new CancellationTokenSource();

            this.consumeMessagesTask = this.ConsumeMessagesContinouslyAsync();
        }

        public async Task SendAsync(Payload payload)
        {
            try
            {
                await this.Connection.SendAsync(payload).ConfigureAwait(false);
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
                await Task.Delay(1_000, this.cancellation.Token).ConfigureAwait(false);

                while (!this.cancellation.IsCancellationRequested)
                {
                    Payload payload = await this.Connection.ReceiveIncomingMessageAsync(this.cancellation.Token).ConfigureAwait(false);

                    if (payload == null)
                    {
                        this.logger.Debug("Error while receiving a message. Disposing the peer.");

                        Task.Run(() => this.Dispose());
                        return;
                    }

                    this.logger.Info("Payload received: " + payload.GetType().Name);

                    try
                    {
                        await this.OnPayloadReceivedAsync(payload, this.cancellation.Token).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("Callback that was processing payloads produced an exception! {0}", e.ToString());
                    }
                }
            }
            catch (OperationCanceledException e)
            {
            }
        }

        protected virtual async Task OnPayloadReceivedAsync(Payload payload, CancellationToken token)
        {
            await this.SendAsync(new CommandNotRecognizedPayload()).ConfigureAwait(false);
        }

        public virtual void Dispose()
        {
            this.logger.Trace("()");

            this.cancellation.Cancel();

            this.consumeMessagesTask.GetAwaiter().GetResult();

            this.cancellation.Dispose();

            Task.Run(() => this.onDisconnectedAndDisposed(this));

            this.logger.Trace("(-)");
        }
    }
}
