using System;
using WatchTool.Common.P2P;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Client.P2P
{
    // sends and receives messages from server
    public class ClientPeer : PeerBase
    {
        public ClientPeer(NetworkConnection connection, Action<PeerBase> onDisconnectedAndDisposed) : base(connection, onDisconnectedAndDisposed)
        {

        }

        protected override void OnPayloadReceived(Payload payload)
        {
            // TODO
        }
    }
}
