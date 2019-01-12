using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
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
        //                ConsensusHeight = 784_587
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

        public IActionResult Request_Update(int peerId)
        {
            this.peersController.SendRequest_Update(peerId);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Request_StartNode(int peerId)
        {
            //this.peersController.SendRequest_Update(peerId);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Request_StopNode(int peerId)
        {
            //this.peersController.SendRequest_Update(peerId);

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
