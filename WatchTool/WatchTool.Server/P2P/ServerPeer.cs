using System;
using System.Threading.Tasks;
using WatchTool.Common.P2P;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Server.P2P
{
    // sends messages to attached client and handles messages for it
    public class ServerPeer : PeerBase
    {
        private readonly ServerConnectionManager connectionManager;

        private readonly Task askForNodeInfoTask;

        /// <summary>How often do we request update from a node.</summary>
        private const int RefreshIntervalSeconds = 10;

        public ServerPeer(NetworkConnection connection, ServerConnectionManager connectionManager, Action<PeerBase> onDisconnectedAndDisposed) : base(connection, onDisconnectedAndDisposed)
        {
            this.connectionManager = connectionManager;

            this.askForNodeInfoTask = AskForInfoContinuously();
        }

        public async Task SendRequest_UpdateAsync()
        {
            this.logger.Trace("()");

            await this.SendAsync(new GetLatestNodeRequestPayload()).ConfigureAwait(false);

            this.logger.Trace("()");
        }

        protected override async Task OnPayloadReceivedAsync(Payload payload)
        {
            switch (payload)
            {
                case NodeInfoPayload nodeInfo:
                    this.connectionManager.OnPeerNodeInfoReceived(nodeInfo, this);
                    break;

                default:
                    await base.OnPayloadReceivedAsync(payload).ConfigureAwait(false);
                    break;
            }
        }

        private async Task AskForInfoContinuously()
        {
            try
            {
                while (!this.cancellation.IsCancellationRequested)
                {
                    await this.SendAsync(new GetInfoRequestPayload()).ConfigureAwait(false);

                    await Task.Delay(RefreshIntervalSeconds * 1_000, this.cancellation.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                this.logger.Error("Error: '{0}'", e.ToString());
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            this.askForNodeInfoTask?.GetAwaiter().GetResult();
        }
    }
}
