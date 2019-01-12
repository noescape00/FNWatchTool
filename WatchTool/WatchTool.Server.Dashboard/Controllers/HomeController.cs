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
        private readonly IPeersController peersInfoProvider;

        public HomeController(IPeersController peersInfoProvider = null) // TODO not null
        {
            this.peersInfoProvider = peersInfoProvider;
        }

        public IActionResult Index()
        {
            var peerInfo = new PeerInfoModel()
            {
                Id = 12,
                EndPoint = new IPEndPoint(15134635, 19879),
                LatestInfoPayload = new NodeInfoPayload()
                {
                    IsNodeCloned = true,
                    IsNodeRunning = true,
                    NodeRepoInfo = new NodeRepositoryVersionInfo()
                    {
                        LatestCommitDate = new DateTime(2019, 1, 3),
                        LatestCommitHash = "hashhashhashhashhashhashhashhashhashhash"
                    },
                    RunningNodeInfo = new RunningNodeInfo()
                    {
                        ConsensusHeight = 784_587
                    }
                }
            };

            PeersInformationModel fakeModel = new PeersInformationModel();
            fakeModel.PeersInfo = new List<PeerInfoModel>()
            {
                peerInfo, peerInfo
            };

            return View(fakeModel);
        }

        //public IActionResult Index()
        //{
        //    PeersInformationModel infoModel = this.peersInfoProvider.GetPeersInfo();
        //
        //    return View(infoModel);
        //}

        public IActionResult Request_Update(int peerId)
        {
            //PeersInformationModel infoModel = this.peersInfoProvider.GetPeersInfo();

            return View();
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
