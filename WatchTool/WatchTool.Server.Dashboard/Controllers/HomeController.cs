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

        public IActionResult Index()
        {
            PeersInformationModel infoModel = this.peersController.GetPeersInfo();

            return View(infoModel);
        }

        public async Task<IActionResult> Request_Update(int peerId)
        {
            this.logger.Trace("()");

            try
            {
                await this.peersController.SendPayloadToPeerAsync(peerId, new GetLatestNodeRequestPayload()).ConfigureAwait(false);

                var data = await this.GetNextPeerUpdateAsync(peerId).ConfigureAwait(false);

                this.logger.Trace("(-)");
                return View("~/Views/Partial/PeerRow.cshtml", data);
            }
            catch (Exception e)
            {
                this.logger.Debug("Exception occured: '{0}'", e.ToString());
                throw;
            }
        }

        public async Task<IActionResult> Request_StartNode(int peerId)
        {
            this.logger.Trace("()");

            try
            {
                await this.peersController.SendPayloadToPeerAsync(peerId, new StartNodeRequestPayload()).ConfigureAwait(false);

                var data = await this.GetNextPeerUpdateAsync(peerId).ConfigureAwait(false);

                this.logger.Trace("(-)");
                return View("~/Views/Partial/PeerRow.cshtml", data);
            }
            catch (Exception e)
            {
                this.logger.Debug("Exception occured: '{0}'", e.ToString());
                throw;
            }
        }

        public async Task<IActionResult> Request_StopNode(int peerId)
        {
            this.logger.Trace("()");

            try
            {
                await this.peersController.SendPayloadToPeerAsync(peerId, new StopNodeRequestPayload()).ConfigureAwait(false);

                var data = await this.GetNextPeerUpdateAsync(peerId).ConfigureAwait(false);

                this.logger.Trace("(-)");
                return View("~/Views/Partial/PeerRow.cshtml", data);
            }
            catch (Exception e)
            {
                this.logger.Debug("Exception occured: '{0}'", e.ToString());
                throw;
            }
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
