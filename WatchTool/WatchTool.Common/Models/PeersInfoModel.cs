using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Common.Models
{
    public interface IPeersController
    {
        PeersInformationModel GetPeersInfo();

        Task SendPayloadToPeerAsync(int peerId, Payload payload);
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
