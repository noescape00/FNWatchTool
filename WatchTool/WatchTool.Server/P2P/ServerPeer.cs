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
        public ServerPeer(NetworkConnection connection, Action<PeerBase> onDisconnectedAndDisposed) : base(connection, onDisconnectedAndDisposed)
        {

        }

        protected override async Task OnPayloadReceivedAsync(Payload payload)
        {
            switch (payload)
            {
                case NodeInfoPayload nodeInfo:
                    // TODO process
                    break;

                default:
                    await base.OnPayloadReceivedAsync(payload).ConfigureAwait(false);
                    break;
            }
        }
    }
}
