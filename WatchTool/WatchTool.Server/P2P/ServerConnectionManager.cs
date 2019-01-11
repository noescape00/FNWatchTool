using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace WatchTool.Server.P2P
{
    public class ServerConnectionManager : IDisposable
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>Protects access to peers collection.</summary>
        private readonly object locker;

        private readonly List<ServerPeer> peers;

        private bool isDisposing = false;

        public ServerConnectionManager()
        {
            this.locker = new object();
            this.peers = new List<ServerPeer>();
        }

        public void AddPeer(ServerPeer peer)
        {
            this.logger.Trace("()");

            if (isDisposing)
            {
                this.logger.Trace("(-)[DISPOSING]");
                return;
            }

            if (!peer.Connection.IsConnected())
                return;

            lock (this.locker)
            {
                this.peers.Add(peer);

                this.LogPeersLocked();
            }

            this.logger.Trace("(-)");
        }

        public void RemovePeer(ServerPeer peer)
        {
            this.logger.Trace("()");

            if (isDisposing)
            {
                this.logger.Trace("(-)[DISPOSING]");
                return;
            }

            lock (this.locker)
            {
                this.peers.Remove(peer);

                this.LogPeersLocked();
            }

            this.logger.Trace("(-)");
        }

        public void Dispose()
        {
            this.logger.Trace("()");

            this.isDisposing = true;

            lock (this.locker)
            {
                foreach (var peer in peers)
                {
                    peer.Dispose();
                }

                peers.Clear();
            }

            this.logger.Trace("(-)");
        }

        private void LogPeersLocked()
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendLine($"=====Connected peers: {peers.Count}");

            foreach (ServerPeer serverPeer in peers)
                builder.AppendLine(serverPeer.Connection.GetConnectionEndPoint().ToString());

            this.logger.Info(builder.ToString());
        }
    }
}
