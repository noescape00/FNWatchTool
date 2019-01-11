using System;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Common.P2P.Payloads
{
    [Payload("info")]
    public class NodeInfoPayload : Payload
    {
        /// <summary>Hash of the latest commit or <c>null</c> if repo was not cloned yet.</summary>
        public string LatestCommitHash { get; set; }

        /// <summary>Date of the latest commit or <c>null</c> if repo was not cloned yet.</summary>
        public DateTime? LatestCommitDate { get; set; }
    }
}
