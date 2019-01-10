using System;
using WatchTool.Common.P2P;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Server.P2P
{
    // sends messages to attached client and handles messages for it
    public class ServerPeer : PeerBase
    {
        public ServerPeer(NetworkConnection connection, Action<PeerBase> onDisconnectedAndDisposed) : base(connection, onDisconnectedAndDisposed)
        {

        }

        protected override void OnPayloadReceived(Payload payload)
        {
            // TODO
        }
    }
}
