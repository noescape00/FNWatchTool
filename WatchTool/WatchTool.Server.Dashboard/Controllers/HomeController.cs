using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NLog;
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

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        public HomeController(IPeersController peersController)
        {
            this.peersController = peersController;
            this.peerUpdatedQueue = new AsyncQueue<PeerInfoModel>();

            this.peersController.AddListener(this);
        }

        public void OnPeerUpdated(PeerInfoModel model)
        {
            this.peerUpdatedQueue.Enqueue(model);
        }

        //// TODO remove
        //public IActionResult Index()
        //{
        //    var m = new PeersInformationModel() {PeersInfo = new List<PeerInfoModel>()};
        //
        //    m.PeersInfo.Add(new PeerInfoModel()
        //    {
        //        EndPoint = new IPEndPoint(1,1),
        //        Id = 13,
        //        LatestInfoPayload = new NodeInfoPayload()
        //        {
        //            IsNodeCloned = false,
        //            IsNodeRunning = false
        //        }
        //    });
        //
        //
        //    return View(m);
        //}

        public IActionResult Index()
        {
            PeersInformationModel infoModel = this.peersController.GetPeersInfo();

            return View(infoModel);
        }

        [HttpPost]
        public async Task<IActionResult> Request_Update(int peerId)
        {
            try
            {
                if (this.peersController != null)
                    await this.peersController.SendPayloadToPeerAsync(peerId, new GetLatestNodeRequestPayload()).ConfigureAwait(false);

                var data = await this.GetNextPeerUpdateAsync(peerId).ConfigureAwait(false);

                return View("~/Views/Partial/PeerRow.cshtml", data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> Request_StartNode(int peerId)
        {
            if (this.peersController != null)
                await this.peersController.SendPayloadToPeerAsync(peerId, new StartNodeRequestPayload()).ConfigureAwait(false);

            var data = await this.GetNextPeerUpdateAsync(peerId).ConfigureAwait(false);

            return View("~/Views/Partial/PeerRow.cshtml", data);
        }

        [HttpPost]
        public async Task<IActionResult> Request_StopNode(int peerId)
        {
            if (this.peersController != null)
                await this.peersController.SendPayloadToPeerAsync(peerId, new StopNodeRequestPayload()).ConfigureAwait(false);

            var data = await this.GetNextPeerUpdateAsync(peerId).ConfigureAwait(false);

            return View("~/Views/Partial/PeerRow.cshtml", data);
        }

        public async Task<PeerInfoModel> GetNextPeerUpdateAsync(int peerId)
        {
            while (true)
            {
                PeerInfoModel item = await this.peerUpdatedQueue.DequeueAsync().ConfigureAwait(false);

                if (item.Id == peerId)
                {
                    return item;
                }
            }
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
            this.peersController.RemoveListener(this);

            base.Dispose(disposing);
        }
    }
}
