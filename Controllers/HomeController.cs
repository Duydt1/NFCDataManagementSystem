using Data.Common;
using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.Extensions.Caching.Distributed;
using NFC.Data.Entities;
using NFC.Data.Models;
using NFC.Models;
using NFC.Services;
using System.Diagnostics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NFC.Controllers
{
	[Authorize]
	public class HomeController(ILogger<HomeController> logger, IServiceProvider serviceProvider) : Controller
	{
		private readonly ILogger<HomeController> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        public async Task<IActionResult> IndexAsync(FilterModel filterModel)
		{
			var cacheKey = $"users";
			var cache = _serviceProvider.GetService<IDistributedCache>();
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

			//if (!string.IsNullOrEmpty(filterModel.SearchString))
			//	ViewData["Searching"] = filterModel.SearchString;

            if (filterModel.FromDate == null)
                filterModel.FromDate = DateTime.Now.Date.AddDays(-1);

            if (filterModel.ToDate == null)
                filterModel.ToDate = DateTime.Now.Date.AddDays(1);

            ViewData["CurrentFromDate"] = filterModel.FromDate;
            ViewData["CurrentToDate"] = filterModel.ToDate;
			//ViewBag.PageSize = filterModel.PageSize == 0 ? filterModel.PageSize = 10 : filterModel.PageSize;
			//if (filterModel.PageNumber < 1) filterModel.PageNumber = 1;
			var nfcService = _serviceProvider.GetService<INFCService>();
			var result = await nfcService.GetNFCDashboard(filterModel);
			return View(result);
		}

		public async Task<IActionResult> GetUpdatedDataAsync(int productionLineId, DateTime fromDate, DateTime toDate, string searching)
		{
			var filterModel = new FilterModel
			{
				ProductionLineId = productionLineId,
				FromDate = fromDate,
				SearchString = searching,
				PageNumber = 1,
				ToDate = toDate
			};

			var nfcService = _serviceProvider.GetService<INFCService>();
			var result = await nfcService.GetNFCDashboard(filterModel);
			
			return Ok(result);
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
