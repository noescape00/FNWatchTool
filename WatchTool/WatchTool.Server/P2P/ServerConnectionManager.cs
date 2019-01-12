using System;
using System.Collections.Generic;
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

        private readonly List<ServerPeer> peers;

        private bool isDisposing = false;

        public ServerConnectionManager()
        {
            this.locker = new object();
            this.peers = new List<ServerPeer>();
        }

        // TODO constantly ask peers for their info model and store them here. Remove when peer disconnects

        public PeersInformationModel GetPeersInfo()
        {
            // TODO

            var peerInfo = new PeerInfoModel()
            {
                EndPoint = new IPEndPoint(1, 1),
                LatestInfoPayload = new NodeInfoPayload()
                {
                    IsNodeCloned = true,
                    IsNodeRunning = false,
                    NodeRepoInfo = new NodeRepositoryVersionInfo()
                    {
                        LatestCommitDate = DateTime.Now,
                        LatestCommitHash = "hashhashhashhashhashhashhashhashhashhash"
                    }
                }
            };

            PeersInformationModel fakeModel = new PeersInformationModel();
            fakeModel.PeersInfo = new List<PeerInfoModel>()
            {
                peerInfo, peerInfo
            };

            return fakeModel;
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
