using System.Net;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WatchTool.Common;

namespace WatchTool.Server
{
    public class ServerConfiguration
    {
        public void Initialize(TextFileConfiguration config)
        {
            this.ServerPort = config.GetOrDefault<int>("serverPort", 18989);

            this.ListenEndPoint = new IPEndPoint(IPAddress.Parse("::ffff:0.0.0.0"), this.ServerPort);
        }

        public int ServerPort { get; private set; }

        public IPEndPoint ListenEndPoint { get; private set; }
    }
}
