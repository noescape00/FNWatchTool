using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Common.P2P.Payloads
{
    [Payload("ping")]
    public class PingPayload : Payload
    {
    }

    [Payload("pong")]
    public class PongPayload : Payload
    {
    }
}
