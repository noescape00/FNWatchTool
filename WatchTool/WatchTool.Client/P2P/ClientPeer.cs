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

            // TODO TEST ONLY. DELETE
            Task.Run(async () => { await this.OnPayloadReceivedAsync(new GetLatestNodeRequestPayload()); });
        }

        protected override async Task OnPayloadReceivedAsync(Payload payload)
        {
            switch (payload)
            {
                case ShutDownNodeRequestPayload _:
                    // TODO
                    break;

                case StartNodeRequestPayload _:
                    // TODO
                    break;

                case GetInfoRequestPayload _:
                    // TODO
                    break;

                case GetLatestNodeRequestPayload _:
                    this.nodeController.StartUpdatingOrCloningTheNode();
                    break;

                default:
                    await base.OnPayloadReceivedAsync(payload).ConfigureAwait(false);
                    break;
            }
        }
    }
}
