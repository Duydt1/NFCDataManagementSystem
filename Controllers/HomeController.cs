using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var repoProductionLine = _serviceProvider.GetService<IProductionLineRepository>();
			var productionLines = await repoProductionLine.GetListNameAsync(userId);
			ViewData["ProductionLines"] = new SelectList(productionLines, "Id", "Name", 1);
			if (productionLines.Count > 0 && filterModel.ProductionLineId == null)
				filterModel.ProductionLineId = productionLines.FirstOrDefault().Id;

			if (!string.IsNullOrEmpty(filterModel.SearchString))
				ViewData["Searching"] = filterModel.SearchString;

            if (filterModel.FromDate == null)
                filterModel.FromDate = DateTime.Now.Date;

            if (filterModel.ToDate == null)
                filterModel.ToDate = DateTime.Now.Date.AddDays(1);

            ViewData["CurrentFromDate"] = filterModel.FromDate;
            ViewData["CurrentToDate"] = filterModel.ToDate;

			ViewBag.PageSize = filterModel.PageSize;

			var nfcService = _serviceProvider.GetService<INFCService>();
			var result = await nfcService.GetNFCDashboard(filterModel);
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
