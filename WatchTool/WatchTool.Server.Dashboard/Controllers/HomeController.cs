using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WatchTool.Common.Models;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Server.Dashboard.Models;

namespace WatchTool.Server.Dashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPeersController peersController;

        public HomeController(IPeersController peersController = null) // TODO not null
        {
            this.peersController = peersController;
        }

        //public IActionResult Index()
        //{
        //    var peerInfo1 = new PeerInfoModel()
        //    {
        //        Id = 3,
        //        EndPoint = new IPEndPoint(15134635, 19879),
        //        LatestInfoPayload = new NodeInfoPayload()
        //        {
        //            IsNodeCloned = false,
        //            IsNodeRunning = false,
        //        }
        //    };
        //
        //
        //    string log = "";
        //    for (int i = 0; i < 55; i++)
        //    {
        //        log += "START -- Peer:[::ffff:23.102.35.247]:26178, connected:outbound, (r/s):717485*/717485* agent:GT-StratisNode:3.0.0 ---FIN" + Environment.NewLine;
        //    }
        //
        //
        //    var peerInfo2 = new PeerInfoModel()
        //    {
        //        Id = 4,
        //        EndPoint = new IPEndPoint(15134635, 19879),
        //        LatestInfoPayload = new NodeInfoPayload()
        //        {
        //            IsNodeCloned = true,
        //            IsNodeRunning = true,
        //            NodeRepoInfo = new NodeRepositoryVersionInfo()
        //            {
        //                LatestCommitDate = new DateTime(2019, 1, 3),
        //                LatestCommitHash = "hashhashhashhashhashhashhashhashhashhash"
        //            },
        //            RunningNodeInfo = new RunningNodeInfo()
        //            {
        //                ConsensusHeight = 784_587,
        //                LastConsoleOutput = log
        //            }
        //        }
        //    };
        //
        //    var peerInfo3 = new PeerInfoModel()
        //    {
        //        Id = 5,
        //        EndPoint = new IPEndPoint(15134635, 19879),
        //        LatestInfoPayload = new NodeInfoPayload()
        //        {
        //            IsNodeCloned = true,
        //            IsNodeRunning = false,
        //            NodeRepoInfo = new NodeRepositoryVersionInfo()
        //            {
        //                LatestCommitDate = new DateTime(2019, 1, 3),
        //                LatestCommitHash = "hashhashhashhashhashhashhashhashhashhash"
        //            }
        //        }
        //    };
        //
        //    PeersInformationModel fakeModel = new PeersInformationModel();
        //    fakeModel.PeersInfo = new List<PeerInfoModel>()
        //    {
        //        peerInfo1, peerInfo2, peerInfo3
        //    };
        //
        //    return View("Index", fakeModel);
        //}

        public IActionResult Index()
        {
            PeersInformationModel infoModel = this.peersController.GetPeersInfo();

            return View(infoModel);
        }

        public async Task<IActionResult> Request_Update(int peerId)
        {
            await this.peersController.SendPayloadToPeerAsync(peerId, new GetLatestNodeRequestPayload());

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Request_StartNode(int peerId)
        {
            await this.peersController.SendPayloadToPeerAsync(peerId, new StartNodeRequestPayload());

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Request_StopNode(int peerId)
        {
            await this.peersController.SendPayloadToPeerAsync(peerId, new StopNodeRequestPayload());

            return RedirectToAction("Index", "Home");
        }

        public IActionResult About()
        {
            ViewData["Message"] = "SOME TEXT";

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
