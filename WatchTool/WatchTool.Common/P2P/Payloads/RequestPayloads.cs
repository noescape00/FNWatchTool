using System;
using System.Collections.Generic;
using System.Text;
using WatchTool.Common.P2P.PayloadsBase;

namespace WatchTool.Common.P2P.Payloads
{
    [Payload("shutdown")]
    public class ShutDownNodeRequestPayload : Payload
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
    [Payload("update")]
    public class UpdateRepositoryRequestPayload : Payload
    {
    }
}
