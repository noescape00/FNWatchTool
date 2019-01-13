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

        protected override void Dispose(bool disposing)
        {
            this.peersController.RemoveListener(this);

            base.Dispose(disposing);
        }
    }
}
