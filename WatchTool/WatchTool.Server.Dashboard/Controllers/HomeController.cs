using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WatchTool.Common;
using WatchTool.Common.Models;
using WatchTool.Common.P2P.Payloads;
using WatchTool.Server.Dashboard.Models;

namespace WatchTool.Server.Dashboard.Controllers
{
    public class HomeController : Controller, IPeerStateUpdateListener
    {
        private readonly IPeersController peersController;

        private readonly AsyncQueue<PeerInfoModel> peerUpdatedQueue;

        public HomeController(IPeersController peersController = null) // TODO
        {
            this.peersController = peersController;
            this.peerUpdatedQueue = new AsyncQueue<PeerInfoModel>();

            this.peersController?.AddListener(this); // TODO remove ?
        }

        public void OnPeerUpdated(PeerInfoModel model)
        {
            this.peerUpdatedQueue.Enqueue(model);
        }

        // TODO remove
        public IActionResult Index()
        {
            var m = new PeersInformationModel() {PeersInfo = new List<PeerInfoModel>()};

            m.PeersInfo.Add(new PeerInfoModel()
            {
                EndPoint = new IPEndPoint(1,1),
                Id = 13,
                LatestInfoPayload = new NodeInfoPayload()
                {
                    IsNodeCloned = false,
                    IsNodeRunning = false
                }
            });


            return View(m);
        }

        //public IActionResult Index()
        //{
        //    PeersInformationModel infoModel = this.peersController.GetPeersInfo();
        //
        //    return View(infoModel);
        //}

        [HttpPost]
        public async Task<IActionResult> Request_Update(int peerId)
        {
            if (this.peersController != null)
                await this.peersController.SendPayloadToPeerAsync(peerId, new GetLatestNodeRequestPayload()).ConfigureAwait(false);

            var data = await this.GetNextPeerUpdateAsync(peerId).ConfigureAwait(false);

            return data;
        }

        [HttpPost]
        public async Task<IActionResult> Request_StartNode(int peerId)
        {
            if (this.peersController != null)
                await this.peersController.SendPayloadToPeerAsync(peerId, new StartNodeRequestPayload()).ConfigureAwait(false);

            var data = await this.GetNextPeerUpdateAsync(peerId).ConfigureAwait(false);

            return data;
        }

        [HttpPost]
        public async Task<IActionResult> Request_StopNode(int peerId)
        {
            if (this.peersController != null)
                await this.peersController.SendPayloadToPeerAsync(peerId, new StopNodeRequestPayload()).ConfigureAwait(false);

            var data = await this.GetNextPeerUpdateAsync(peerId).ConfigureAwait(false);

            return data;
        }

        public async Task<IActionResult> GetNextPeerUpdateAsync(int peerId) // TODO
        {

            await Task.Delay(2000);

            PeerInfoModel model = new PeerInfoModel()
            {
                EndPoint = new IPEndPoint(55,55),
                Id = 13,
                LatestInfoPayload = new NodeInfoPayload()
                {
                    IsNodeCloned = false,
                    IsNodeRunning = false,
                    RunningNodeInfo = new RunningNodeInfo(),
                    NodeRepoInfo = new NodeRepositoryVersionInfo()
                }
            };

            return View("~/Views/Partial/PeerRow.cshtml", model);
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

        protected override void Dispose(bool disposing)
        {
            this.peersController?.RemoveListener(this); // TODO remove ?

            base.Dispose(disposing);
        }
    }
}
