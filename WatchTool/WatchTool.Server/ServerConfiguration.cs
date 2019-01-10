using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace WatchTool.Server
{
    public static class ServerConfiguration
    {
        public const int ServerPort = 18989;

        public static IPEndPoint ListenEndPoint = new IPEndPoint(IPAddress.Parse("::ffff:127.0.0.1"), ServerPort);
    }
}
