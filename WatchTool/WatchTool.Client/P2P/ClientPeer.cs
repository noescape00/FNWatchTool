using System;
using System.Threading.Tasks;
using WatchTool.Common.P2P;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Client.P2P
{
    // sends and receives messages from server
    public class ClientPeer : PeerBase
    {
        public ClientPeer(NetworkConnection connection, Action<PeerBase> onDisconnectedAndDisposed) : base(connection, onDisconnectedAndDisposed)
        {

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

                case UpdateRepositoryRequestPayload _:
                    // TODO
                    break;

                default:
                    await base.OnPayloadReceivedAsync(payload).ConfigureAwait(false);
                    break;
            }
        }
    }
}
