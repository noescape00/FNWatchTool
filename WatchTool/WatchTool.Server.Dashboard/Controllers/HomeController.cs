using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WatchTool.Common.Models;
using WatchTool.Server.Dashboard.Models;

namespace WatchTool.Server.Dashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly IPeersInformationModelProvider peersInfoProvider;

        public HomeController(IPeersInformationModelProvider peersInfoProvider)
        {
            this.peersInfoProvider = peersInfoProvider;
        }

        public IActionResult Index()
        {
            PeersInformationModel infoModel = this.peersInfoProvider.GetPeersInfo();

            return View(infoModel);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
