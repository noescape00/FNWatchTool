using System;
using System.Collections.Generic;
using System.Text;
using WatchTool.Common.P2P;

namespace WatchTool.Server.P2P
{
    // sends messages to attached client and handles messages for it
    public class ServerPeer : PeerBase
    {
        public ServerPeer(NetworkConnection connection) : base(connection)
        {

        }
    }
}
