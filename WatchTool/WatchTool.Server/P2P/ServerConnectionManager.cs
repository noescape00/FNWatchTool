using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NLog;
using WatchTool.Common.Models;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Server.P2P
{
    public class ServerConnectionManager : IDisposable, IPeersController
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>Protects access to peers collection.</summary>
        private readonly object locker;

        /// <remarks>Should be protected by <see cref="locker"/></remarks>
        private readonly List<ServerPeer> peers;

        /// <remarks>Should be protected by <see cref="locker"/></remarks>
        private readonly Dictionary<int, PeerInfoModel> peerInfoByPeerId;

        /// <remarks>Should be protected by <see cref="locker"/></remarks>
        private readonly List<IPeerStateUpdateListener> listeners;

        private bool isDisposing = false;

        public ServerConnectionManager()
        {
            this.locker = new object();
            this.peers = new List<ServerPeer>();

            this.peerInfoByPeerId = new Dictionary<int, PeerInfoModel>();
            this.listeners = new List<IPeerStateUpdateListener>();
        }

        public async Task SendPayloadToPeerAsync(int peerId, Payload payload)
        {
            this.logger.Trace("()");

            ServerPeer peer = null;

            lock (this.locker)
            {
                peer = this.peers.SingleOrDefault(x => x.Connection.GetId() == peerId);

                if (peer == null)
                {
                    this.logger.Warn("Peer doesn't exist.");
                    return;
                }
            }

            await peer.SendAsync(payload).ConfigureAwait(false);

            this.logger.Trace("(-)");
        }

        public void OnPeerNodeInfoReceived(NodeInfoPayload nodeInfo, ServerPeer peer)
        {
            this.logger.Trace("()");

            List<IPeerStateUpdateListener> listenersCopy;
            PeerInfoModel infoModel;

            lock (this.locker)
            {
                int id = peer.Connection.GetId();

                infoModel = new PeerInfoModel()
                {
                    Id = id,
                    EndPoint = peer.Connection.GetConnectionEndPoint(),
                    LatestInfoPayload = nodeInfo
                };
                this.peerInfoByPeerId[id] = infoModel;

                listenersCopy = new List<IPeerStateUpdateListener>(this.listeners);
            }

            foreach (IPeerStateUpdateListener listener in listenersCopy)
                listener.OnPeerUpdated(infoModel);

            this.logger.Trace("(-)");
        }

        public void AddListener(IPeerStateUpdateListener listener)
        {
            lock (locker)
            {
                this.listeners.Add(listener);
            }
        }

        public void RemoveListener(IPeerStateUpdateListener listener)
        {
            lock (locker)
            {
                this.listeners.Remove(listener);
            }
        }

        public PeersInformationModel GetPeersInfo()
        {
            this.logger.Trace("()");

            lock (this.locker)
            {
                PeersInformationModel model = new PeersInformationModel()
                {
                    PeersInfo = this.peerInfoByPeerId.Values.Where(x => x != null).ToList()
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

                this.peerInfoByPeerId.Add(peer.Connection.GetId(), null);

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
                bool removedPeerInfo = this.peerInfoByPeerId.Remove(peer.Connection.GetId());

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
