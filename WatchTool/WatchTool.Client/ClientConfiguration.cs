using System;
using System.Net;

namespace WatchTool.Client
{
    public static class ClientConfiguration
    {
        public const string ServerIP = "127.0.0.1";
        public const int ServerPort = 18989;

        public static readonly IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Parse(ClientConfiguration.ServerIP), ClientConfiguration.ServerPort);

        /// <summary>Where do we store the repository and other data.</summary>
        public static readonly string WorkFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\WatchTool_StratisFNRepo";

        /// <summary>Where do we clone FN repository from.</summary>
        public const string RepoPath = "https://github.com/stratisproject/StratisBitcoinFullNode.git";

        public const int ConnectToServerRetryDelaySeconds = 5; // TODO change to 20-30

        /// <summary>Port on which node's API is running.</summary>
        public const int ApiPort = 38221;
    }
}
