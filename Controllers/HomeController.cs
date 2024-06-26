using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Models;
using NFC.Services;
using System.Diagnostics;
using System.Security.Claims;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NFC.Controllers
{
	[Authorize]
	public class HomeController(ILogger<HomeController> logger, NFCDbContext context, INFCService nfcService) : Controller
	{
		private readonly ILogger<HomeController> _logger = logger;
		private readonly NFCDbContext _context = context;
		private readonly INFCService _nfcService = nfcService;

		public async Task<IActionResult> IndexAsync(FilterModel filterModel)
		{
			var users = await _context.Users.ToListAsync();
			var historyUploads = await _context.HistoryUploads.ToListAsync();
			ViewData["UserCount"] = users.Count;
			ViewData["HistoryUploadCount"] = historyUploads.Count;
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = users.FirstOrDefault(x => x.Id == userId);
			var result = new List<NFCModel>();
			if (User.IsInRole("Admin"))
				filterModel.ProductionLineId = 0;
			else
				filterModel.ProductionLineId =  user!.ProductionLineId;
			if (!string.IsNullOrEmpty(filterModel.SearchString))
				ViewData["Searching"] = filterModel.SearchString;

			if (!filterModel.FromDate.HasValue)
				filterModel.FromDate = DateTime.Now.AddMonths(-2);

			ViewData["CurrentFromDate"] = filterModel.FromDate;

			if (!filterModel.ToDate.HasValue)
				filterModel.ToDate = DateTime.Now;
			ViewData["CurrentToDate"] = filterModel.ToDate;

			result = await _nfcService.GetNFCDashboard(filterModel);
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
