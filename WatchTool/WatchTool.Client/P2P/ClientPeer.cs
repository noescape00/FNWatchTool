using System;
using System.Collections.Generic;
using System.Text;
using WatchTool.Common.P2P;

namespace WatchTool.Client.P2P
{
    // sends and receives messages from server
    public class ClientPeer : PeerBase
    {
        public ClientPeer(NetworkConnection connection) : base(connection)
        {

        }
    }
}
