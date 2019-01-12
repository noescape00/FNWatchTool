using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Common.P2P.Payloads
{
    [Payload("stop")]
    public class StopNodeRequestPayload : Payload
    {
    }

    [Payload("start")]
    public class StartNodeRequestPayload : Payload
    {
    }

    [Payload("getinfo")]
    public class GetInfoRequestPayload : Payload
    {
    }

    // request to update to latest master or clone the repo
    [Payload("getlatest")]
    public class GetLatestNodeRequestPayload : Payload
    {
    }
}
