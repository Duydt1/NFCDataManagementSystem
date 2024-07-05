using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NFC.Data.Models;
using NFC.Models;
using NFC.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace NFC.Controllers
{
    [Authorize]
	public class HomeController(ILogger<HomeController> logger, IServiceProvider serviceProvider) : Controller
	{
		private readonly ILogger<HomeController> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        public async Task<IActionResult> IndexAsync(FilterModel filterModel)
		{
			var repoIdentity = _serviceProvider.GetService<IIdentityRepository>();
			var repoHistoryUpload = _serviceProvider.GetService<IHistoryUploadRepository>();
			var historyUploads = await repoHistoryUpload.GetAllAsync();
			var users = await repoIdentity.GetAllUserAsync();
            ViewData["UserCount"] = users.Count();
			ViewData["HistoryUploadCount"] = historyUploads.Count();
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = users.FirstOrDefault(x => x.Id == userId);
			var result = new List<NFCModel>();
			if (User.IsInRole("Admin"))
				filterModel.ProductionLineId = 0;
			else
				filterModel.ProductionLineId =  user!.ProductionLineId;
			if (!string.IsNullOrEmpty(filterModel.SearchString))
				ViewData["Searching"] = filterModel.SearchString;

            if (filterModel.FromDate == null)
                filterModel.FromDate = DateTime.Now.Date;

            if (filterModel.ToDate == null)
                filterModel.ToDate = DateTime.Now.Date.AddDays(1);

            ViewData["CurrentFromDate"] = filterModel.FromDate;
            ViewData["CurrentToDate"] = filterModel.ToDate;

			var nfcService = _serviceProvider.GetService<INFCService>();
			result = await nfcService.GetNFCDashboard(filterModel);
			return View(result);
		}
		public async Task<IActionResult> PrivacyAsync()
		{

			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		
	}
}
