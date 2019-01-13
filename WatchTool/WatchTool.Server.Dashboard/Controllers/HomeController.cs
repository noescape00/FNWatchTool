using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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
            return View(new PeersInformationModel() {PeersInfo = new List<PeerInfoModel>()});
        }

        //public IActionResult Index()
        //{
        //    PeersInformationModel infoModel = this.peersController.GetPeersInfo();
        //
        //    return View(infoModel);
        //}


        // TODO test
        [HttpPost]
        public async Task<IActionResult> WaitForPeerBeingUpdated(int peerId)
        {
            await Task.Delay(1000);

            return Json(5);
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

        protected override void Dispose(bool disposing)
        {
            this.peersController?.RemoveListener(this); // TODO remove ?

            base.Dispose(disposing);
        }
    }
}
