using System.Net;
using WatchTool.Common;

namespace WatchTool.Server
{
    public class ServerConfiguration
    {
        public void Initialize(TextFileConfiguration config)
        {
            this.ServerPort = config.GetOrDefault<int>("serverPort", 18989);

            this.ListenEndPoint = new IPEndPoint(IPAddress.Parse("::ffff:0.0.0.0"), this.ServerPort);

            this.RefreshIntervalSeconds = config.GetOrDefault<int>("refreshInterval", 20);
        }

        /// <summary>Delay between requests for an update from a client.</summary>
        public int RefreshIntervalSeconds { get; private set; }

        public int ServerPort { get; private set; }

        public IPEndPoint ListenEndPoint { get; private set; }
    }
}
