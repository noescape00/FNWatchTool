using System;
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

        protected override async Task OnPayloadReceivedAsync(Payload payload)
        {
            switch (payload)
            {
                case StartNodeRequestPayload _:
                    this.nodeController.RunNode();
                    await this.SendInfoPayload().ConfigureAwait(false);
                    break;

                case StopNodeRequestPayload _:
                    await this.nodeController.StopNodeAsync().ConfigureAwait(false);
                    await this.SendInfoPayload().ConfigureAwait(false);
                    break;

                case GetInfoRequestPayload _:
                    await this.SendInfoPayload().ConfigureAwait(false);
                    break;

                case GetLatestNodeRequestPayload _:
                    this.nodeController.StartUpdatingOrCloningTheNode(async () => await this.SendInfoPayload().ConfigureAwait(false));
                    break;

                default:
                    await base.OnPayloadReceivedAsync(payload).ConfigureAwait(false);
                    break;
            }
        }

        private async Task SendInfoPayload()
        {
            NodeInfoPayload nodeInfo = this.nodeController.GetNodeInfo();

            await this.SendAsync(nodeInfo).ConfigureAwait(false);
        }
    }
}
