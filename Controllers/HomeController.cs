using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Models;
using NFC.Services;
using System.Diagnostics;
using System.Security.Claims;

namespace NFC.Controllers
{
	[Authorize]
	public class HomeController(ILogger<HomeController> logger, NFCDbContext context, INFCService nfcService) : Controller
	{
		private readonly ILogger<HomeController> _logger = logger;
		private readonly NFCDbContext _context = context;
		private readonly INFCService _nfcService = nfcService;

		public async Task<IActionResult> IndexAsync(int? productionLineId)
		{
			var users = await _context.Users.ToListAsync();
			var historyUploads = await _context.HistoryUploads.ToListAsync();
			ViewData["UserCount"] = users.Count;
			ViewData["HistoryUploadCount"] = historyUploads.Count;
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var user = users.FirstOrDefault(x => x.Id == userId);
			var result = new List<NFCModel>();
			var productionLines = new List<int>();
			if (User.IsInRole("Admin"))
				productionLines = await GetProductionLines(null);
			else
				productionLines = await GetProductionLines(user!.ProductionLineId);

			result = await _nfcService.GetNFCDashboard(productionLines!);
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

		private async Task<List<int>> GetProductionLines(int? productionLineId)
		{
			var query = _context.ProductionLines.AsQueryable();
			if(productionLineId != null)
			{
				query = query.Where(x => x.Id == productionLineId);
			}

			return await query.Select(x => x.Id).ToListAsync();
		}
	}
}
