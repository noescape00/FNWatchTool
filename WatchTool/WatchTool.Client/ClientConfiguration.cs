using System;
using System.IO;
using System.Net;
using WatchTool.Common;

namespace WatchTool.Client
{
    public class ClientConfiguration
    {
        public void Initialize(TextFileConfiguration config)
        {
            this.ServerIP = config.GetOrDefault<string>("serverIp", "52.178.220.228"); //127.0.0.1   52.178.220.228

            this.ServerPort = config.GetOrDefault<int>("serverPort", 18989);

            this.ServerEndPoint = new IPEndPoint(IPAddress.Parse(this.ServerIP), this.ServerPort);

            this.ApiPort = config.GetOrDefault<int>("apiPort", 38221);

            this.WorkFolder = config.GetOrDefault<string>("workFolder", null);

            if (this.WorkFolder == null)
                this.WorkFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WatchTool_StratisFNRepo");

            this.RepoPath = config.GetOrDefault<string>("repoPath", "https://github.com/stratisproject/StratisBitcoinFullNode.git");
        }

        public string ServerIP { get; private set; }

        public int ServerPort { get; private set; }

        public IPEndPoint ServerEndPoint { get; private set; }

        /// <summary>Port on which node's API is running.</summary>
        public int ApiPort { get; private set; }

        /// <summary>Where do we store the repository and other data.</summary>
        public string WorkFolder { get; private set; }

        /// <summary>Where do we clone FN repository from.</summary>
        public string RepoPath { get; private set; }

        public int ConnectToServerRetryDelaySeconds { get; private set; } = 10;
    }
}
