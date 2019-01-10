using System.Net;

namespace WatchTool.Client
{
    public static class Configuration
    {
        public const string ServerIP = "127.0.0.1";
        public const int ServerPort = 18989;

        public static readonly IPEndPoint ServerEndPoint = new IPEndPoint(IPAddress.Parse(Configuration.ServerIP), Configuration.ServerPort);
    }
}
