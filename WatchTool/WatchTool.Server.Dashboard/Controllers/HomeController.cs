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
        private readonly IPeersInformationModelProvider peersInfoProvider;

        public HomeController(IPeersInformationModelProvider peersInfoProvider = null) // TODO not null
        {
            this.peersInfoProvider = peersInfoProvider;
        }

        public IActionResult Index()
        {
            //PeersInformationModel infoModel = this.peersInfoProvider.GetPeersInfo();


            var peerInfo = new PeerInfoModel()
            {
                EndPoint = new IPEndPoint(15134635, 19879),
                LatestInfoPayload = new NodeInfoPayload()
                {
                    IsNodeCloned = true,
                    IsNodeRunning = false,
                    NodeRepoInfo = new NodeRepositoryVersionInfo()
                    {
                        LatestCommitDate = DateTime.Now,
                        LatestCommitHash = "hashhashhashhashhashhashhashhashhashhash"
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
