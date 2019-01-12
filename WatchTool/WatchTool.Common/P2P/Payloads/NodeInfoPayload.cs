using System;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Common.P2P.Payloads
{
    [Payload("info")]
    public class NodeInfoPayload : Payload
    {
        public bool IsNodeCloned { get; set; }

        /// <summary>Info about node's repo or <c>null</c> if <see cref="IsNodeCloned"/> is <c>false</c>.</summary>
        public NodeRepositoryVersionInfo NodeRepoInfo { get; set; }

        public bool IsNodeRunning { get; set; }

        /// <summary>Info about node's running state or <c>null</c> if <see cref="IsNodeRunning"/> is <c>false</c>.</summary>
        public RunningNodeInfo RunningNodeInfo { get; set; }
    }

    public class NodeRepositoryVersionInfo
    {
        /// <summary>Hash of the latest commit.</summary>
        public string LatestCommitHash { get; set; }

        /// <summary>Date of the latest commit.</summary>
        public DateTime LatestCommitDate { get; set; }
    }

    public class RunningNodeInfo
    {
        public int ConsensusHeight { get; set; }

        public string LastConsoleOutput { get; set; }
    }
}
