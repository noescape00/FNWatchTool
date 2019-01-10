using System;
using System.Collections.Generic;
using System.Text;

namespace WatchTool.Common.P2P
{
    // sends messages to attached connection and handles messages for it
    public abstract class PeerBase
    {
        private readonly NetworkConnection connection;

        protected PeerBase(NetworkConnection connection)
        {
            this.connection = connection;
        }

        // TODO ping\pong
    }
}
