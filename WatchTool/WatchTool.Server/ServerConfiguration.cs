using System.Net;

namespace WatchTool.Server
{
    public static class ServerConfiguration
    {
        public const int ServerPort = 18989;

        public static IPEndPoint ListenEndPoint = new IPEndPoint(IPAddress.Parse("::ffff:127.0.0.1"), ServerPort);
    }
}
