using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using NLog;
using WatchTool.Common.Models;
using WatchTool.Common.P2P.Payloads;

namespace WatchTool.Server.P2P
{
    public class ServerConnectionManager : IDisposable, IPeersInformationModelProvider
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>Protects access to peers collection.</summary>
        private readonly object locker;

        /// <remarks>Should be protected by <see cref="locker"/></remarks>
        private readonly List<ServerPeer> peers;

        /// <remarks>Should be protected by <see cref="locker"/></remarks>
        private readonly Dictionary<EndPoint, PeerInfoModel> peerInfo;

        private bool isDisposing = false;

        public ServerConnectionManager()
        {
            this.locker = new object();
            this.peers = new List<ServerPeer>();

            this.peerInfo = new Dictionary<EndPoint, PeerInfoModel>();
        }

        // TODO constantly ask peers for their info model and store them here. Remove when peer disconnects

        // TODO latest peer data container here!

        public void OnPeerNodeInfoReceived(NodeInfoPayload nodeInfo, ServerPeer peer)
        {
            this.logger.Trace("()");

            lock (this.locker)
            {
                EndPoint endPoint = peer.Connection.GetConnectionEndPoint();

                this.peerInfo[endPoint] = new PeerInfoModel()
                {
                    EndPoint = endPoint,
                    LatestInfoPayload = nodeInfo
                };
            }

            this.logger.Trace("(-)");
        }

        public PeersInformationModel GetPeersInfo()
        {
            this.logger.Trace("()");

            lock (this.locker)
            {
                PeersInformationModel model = new PeersInformationModel()
                {
                    PeersInfo = this.peerInfo.Values.ToList()
                };

                this.logger.Trace("(-)");
                return model;
            }
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

                this.peerInfo.Add(peer.Connection.GetConnectionEndPoint(), null);

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
                bool removedPeer = this.peers.Remove(peer);
                bool removedPeerInfo = this.peerInfo.Remove(peer.Connection.GetConnectionEndPoint());

                if (!removedPeer || !removedPeerInfo)
                    this.logger.Warn("Unexpected. Removed peer: {0}, removed peer info: {1}", removedPeer, removedPeerInfo);

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
