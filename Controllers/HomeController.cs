using Data.Common;
using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using NFC.Data.Entities;
using NFC.Data.Models;
using NFC.Models;
using NFC.Services;
using System.Diagnostics;

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
			var cacheKey = $"users";
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var users = await cache.GetRecordAsync<List<NFCUser>>(cacheKey);
			if (users == null)
			{
				users = await repoIdentity.GetAllUserAsync();
				await cache.SetRecordAsync(cacheKey, users, TimeSpan.FromDays(1));
			}
			ViewData["UserCount"] = users.Count();
			ViewData["HistoryUploadCount"] = historyUploads.Count();

			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var productionLinesCacheKey = $"productionLines_{userId}";
			var repoProductionLine = _serviceProvider.GetService<IProductionLineRepository>();
			var productionLines = await cache.GetRecordAsync<List<ProductionLine>>(productionLinesCacheKey);
			if (productionLines == null)
			{
				productionLines = await repoProductionLine.GetAllAsync(userId);
				await cache.SetRecordAsync(productionLinesCacheKey, productionLines, TimeSpan.FromDays(1));
			}

			ViewData["ProductionLines"] = new SelectList(productionLines, "Id", "Name", 1);
			if (productionLines.Count > 0 && filterModel.ProductionLineId == null)
				filterModel.ProductionLineId = productionLines.FirstOrDefault().Id;

			if (!string.IsNullOrEmpty(filterModel.SearchString))
				ViewData["Searching"] = filterModel.SearchString;

            if (filterModel.FromDate == null)
                filterModel.FromDate = DateTime.Now.Date.AddDays(-1);

            if (filterModel.ToDate == null)
                filterModel.ToDate = DateTime.Now.Date.AddDays(1);

            ViewData["CurrentFromDate"] = filterModel.FromDate;
            ViewData["CurrentToDate"] = filterModel.ToDate;
			ViewBag.PageSize = filterModel.PageSize == 0 ? filterModel.PageSize = 10 : filterModel.PageSize;
			if (filterModel.PageNumber < 1) filterModel.PageNumber = 1;
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
