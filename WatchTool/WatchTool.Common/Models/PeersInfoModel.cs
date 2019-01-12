using System.Collections.Generic;
using System.Net;
using WatchTool.Common.P2P.Payloads;

namespace WatchTool.Common.Models
{
    public interface IPeersController
    {
        PeersInformationModel GetPeersInfo();

        void SendRequest_Update(int peerId);
    }

    public class PeersInformationModel
    {
        public List<PeerInfoModel> PeersInfo;
    }

    public class PeerInfoModel
    {
        public int Id { get; set; }

        public NodeInfoPayload LatestInfoPayload { get; set; }

        public EndPoint EndPoint { get; set; }
    }
}
