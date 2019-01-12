using System.Collections.Generic;
using System.Net;
using WatchTool.Common.P2P.Payloads;

namespace WatchTool.Common.Models
{
    public interface IPeersInformationModelProvider
    {
        PeersInformationModel GetPeersInfo();
    }

    public class PeersInformationModel
    {
        public List<PeerInfoModel> PeersInfo;
    }

    public class PeerInfoModel
    {
        public NodeInfoPayload LatestInfoPayload { get; set; }

        public EndPoint EndPoint { get; set; }
    }
}
