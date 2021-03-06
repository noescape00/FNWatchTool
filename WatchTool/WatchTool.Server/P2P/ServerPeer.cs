﻿using System;
using System.Threading;
using System.Threading.Tasks;
using WatchTool.Common;
using WatchTool.Common.P2P;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Server.P2P
{
    // sends messages to attached client and handles messages for it
    public class ServerPeer : PeerBase
    {
        private readonly ServerConnectionManager connectionManager;

        private readonly Task askForNodeInfoTask;

        private readonly ServerConfiguration configuration;

        private readonly AsyncManualResetEvent resetEvent;

        public ServerPeer(NetworkConnection connection, ServerConnectionManager connectionManager, ServerConfiguration configuration, Action<PeerBase> onDisconnectedAndDisposed) : base(connection, onDisconnectedAndDisposed)
        {
            this.connectionManager = connectionManager;
            this.configuration = configuration;

            this.resetEvent = new AsyncManualResetEvent(true);
            this.askForNodeInfoTask = AskForInfoContinuously();
        }

        protected override async Task OnPayloadReceivedAsync(Payload payload, CancellationToken token)
        {
            switch (payload)
            {
                case NodeInfoPayload nodeInfo:
                    this.resetEvent.Set();
                    this.connectionManager.OnPeerNodeInfoReceived(nodeInfo, this);
                    break;
            }
        }

        private async Task AskForInfoContinuously()
        {
            try
            {
                while (!this.cancellation.IsCancellationRequested)
                {
                    this.resetEvent.Reset();
                    await this.SendAsync(new GetInfoRequestPayload()).ConfigureAwait(false);

                    await this.resetEvent.WaitAsync(this.cancellation.Token).ConfigureAwait(false);

                    await Task.Delay(this.configuration.RefreshIntervalSeconds * 1_000, this.cancellation.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                this.logger.Error("Error: '{0}'", e.ToString());
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            this.askForNodeInfoTask?.GetAwaiter().GetResult();
        }
    }
}
