using System;
using System.Threading;
using System.Threading.Tasks;
using WatchTool.Client.NodeIntegration;
using WatchTool.Common.P2P;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Client.P2P
{
    // sends and receives messages from server
    public class ClientPeer : PeerBase
    {
        private readonly NodeController nodeController;

        public ClientPeer(NetworkConnection connection, NodeController nodeController, Action<PeerBase> onDisconnectedAndDisposed) : base(connection, onDisconnectedAndDisposed)
        {
            this.nodeController = nodeController;
        }

        protected override async Task OnPayloadReceivedAsync(Payload payload, CancellationToken token)
        {
            switch (payload)
            {
                case StartNodeRequestPayload _:
                    await this.nodeController.RunNodeAsync(token).ConfigureAwait(false);
                    await this.SendInfo(token).ConfigureAwait(false);
                    break;

                case StopNodeRequestPayload _:
                    await this.nodeController.StopNodeAsync(token).ConfigureAwait(false);
                    await this.SendInfo(token).ConfigureAwait(false);
                    break;

                case GetLatestNodeRequestPayload _:
                    await this.nodeController.UpdateOrCloneTheNodeAsync().ConfigureAwait(false);
                    await this.SendInfo(token).ConfigureAwait(false);
                    break;

                case GetInfoRequestPayload _:
                    await this.SendInfo(token).ConfigureAwait(false);
                    break;

                default:
                    await base.OnPayloadReceivedAsync(payload, token).ConfigureAwait(false);
                    break;
            }
        }

        private async Task SendInfo(CancellationToken token)
        {
            NodeInfoPayload nodeInfo = await this.nodeController.GetNodeInfo(token).ConfigureAwait(false);

            await this.SendAsync(nodeInfo).ConfigureAwait(false);
        }
    }
}
