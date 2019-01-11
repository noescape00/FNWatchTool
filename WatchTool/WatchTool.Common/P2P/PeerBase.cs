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

        private readonly CancellationTokenSource cancellation;

        private Task consumeMessagesTask;

        protected readonly Logger logger = LogManager.GetCurrentClassLogger();

        private const int PingIntervalSeconds = 60;
        private const int PingMaxAnswerDelaySeconds = PingIntervalSeconds / 2;

        private Task pingingTask;
        private DateTime lastTimePongReceived;

        protected PeerBase(NetworkConnection connection, Action<PeerBase> onDisconnectedAndDisposed)
        {
            this.Connection = connection;
            this.onDisconnectedAndDisposed = onDisconnectedAndDisposed;

            this.cancellation = new CancellationTokenSource();

            this.consumeMessagesTask = this.ConsumeMessagesContinouslyAsync();

            this.lastTimePongReceived = DateTime.MinValue;
            this.pingingTask = this.PingContinouslyAsync();
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
                while (!this.cancellation.IsCancellationRequested)
                {
                    Payload payload = await this.Connection.ReceiveIncomingMessageAsync(this.cancellation.Token).ConfigureAwait(false);

                    if (payload == null)
                    {
                        this.logger.Debug("Error while receiving a message. Disposing the peer.");

                        Task.Run(() => this.Dispose());
                        return;
                    }

                    try
                    {
                        await this.OnPayloadReceivedAsync(payload).ConfigureAwait(false);
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

        protected virtual async Task OnPayloadReceivedAsync(Payload payload)
        {
            switch (payload)
            {
                case PingPayload _:
                    this.logger.Debug("Ping received. Answering with pong.");
                    await this.SendAsync(new PongPayload());
                    break;

                case PongPayload _:
                    this.logger.Debug("Pong received.");
                    this.lastTimePongReceived = DateTime.Now;
                    break;
            }
        }

        private async Task PingContinouslyAsync()
        {
            try
            {
                while (!this.cancellation.IsCancellationRequested)
                {
                    this.logger.Debug("Sending ping");

                    var latestTimeToAnswerPing = DateTime.Now + TimeSpan.FromSeconds(PingMaxAnswerDelaySeconds);
                    await this.SendAsync(new PingPayload()).ConfigureAwait(false);

                    Task delayTillNextSend = Task.Delay(PingIntervalSeconds * 1000, this.cancellation.Token);

                    // Wait for pong and check it.
                    await Task.Delay(TimeSpan.FromSeconds(PingMaxAnswerDelaySeconds), this.cancellation.Token).ConfigureAwait(false);

                    bool good = this.lastTimePongReceived < latestTimeToAnswerPing;


                    if (!good)
                    {
                        this.logger.Warn("Ping message was ignored for {0} seconds! Disconnecting.", PingMaxAnswerDelaySeconds);

                        Task.Run(() => this.Dispose());
                        return;
                    }

                    await delayTillNextSend.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        public void Dispose()
        {
            this.logger.Trace("()");

            this.cancellation.Cancel();

            this.pingingTask.GetAwaiter().GetResult();
            this.consumeMessagesTask.GetAwaiter().GetResult();

            this.cancellation.Dispose();

            Task.Run(() => this.onDisconnectedAndDisposed(this));

            this.logger.Trace("(-)");
        }
    }
}
