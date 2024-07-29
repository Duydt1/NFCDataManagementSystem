using Data.Common;
using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NFC.Data.Entities;
using NFC.Data.Models;
using NuGet.Protocol.Core.Types;
using static NFC.Data.Common.NFCUtil;

namespace NFC.Controllers
{
	[Authorize]
	public class HearingsController(IServiceProvider serviceProvider) : Controller
	{
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        // GET: Hearings
        public async Task<IActionResult> Index(FilterModel filterModel)
		{
			if (!string.IsNullOrEmpty(filterModel.SearchString))
				ViewData["Searching"] = filterModel.SearchString;

            if (filterModel.FromDate == null)
                filterModel.FromDate = DateTime.Now.Date.AddDays(-1);

            if (filterModel.ToDate == null)
                filterModel.ToDate = DateTime.Now.Date.AddDays(1);

            ViewData["CurrentFromDate"] = filterModel.FromDate;
			ViewData["CurrentToDate"] = filterModel.ToDate;
			var cache = _serviceProvider.GetService<IDistributedCache>();

			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

			var productionLinesCacheKey = $"productionLines_{userId}";
			var productionLines = await cache.GetRecordAsync<List<ProductionLine>>(productionLinesCacheKey);
			if (productionLines == null)
			{
				var repo = _serviceProvider.GetService<IProductionLineRepository>();
				productionLines = await repo.GetAllAsync(userId);
				await cache.SetRecordAsync(productionLinesCacheKey, productionLines, TimeSpan.FromDays(1));
			}
			ViewData["ProductionLines"] = new SelectList(productionLines, "Id", "Name", 1);
			ViewBag.PageSize = filterModel.PageSize == 0 ? filterModel.PageSize = 10 : filterModel.PageSize;
			if (productionLines.Count > 0 && filterModel.ProductionLineId == null)
				filterModel.ProductionLineId = productionLines.FirstOrDefault().Id;

			if (filterModel.PageNumber < 1) filterModel.PageNumber = 1;
			var repository = _serviceProvider.GetService<IHearingRepository>();
			var results = await repository.GetAllAsync(filterModel);
			GetDayShiftCount(results);
			GetNightShiftCount(results);
			return View(results);
		}
		private string GetCacheKey(FilterModel filterModel)
		{
			return $"hearings_{filterModel.FromDate?.ToString("yyyy-MM-dd")}_{filterModel.ToDate?.ToString("yyyy-MM-dd")}_{filterModel.ProductionLineId}_{filterModel.PageSize}_{filterModel.PageNumber}";
		}
		private void GetDayShiftCount(PaginatedList<Hearing> results)
		{
			var dayShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(8, 0, 0) && x.DateTime.TimeOfDay < new TimeSpan(20, 0, 0));
			var countPass = dayShift.Where(x => x.Result!.Equals("PASS", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = dayShift.Where(x => x.Result!.Equals("FAIL", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = dayShift.Count();
			ViewData["ToTalDayPass"] = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			ViewData["ToTalDayFail"] = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
		}

		private void GetNightShiftCount(PaginatedList<Hearing> results)
		{
			var nightShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(20, 0, 0) || x.DateTime.TimeOfDay < new TimeSpan(8, 0, 0));
			var countPass = nightShift.Where(x => x.Result!.Equals("PASS", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = nightShift.Where(x => x.Result!.Equals("FAIL", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = nightShift.Count();
			ViewData["ToTalNightPass"] = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			ViewData["ToTalNightFail"] = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
		}

		// GET: Hearings/Details/5
		public async Task<IActionResult> Details(long id)
		{
            var repository = _serviceProvider.GetService<IHearingRepository>();
            var entity = await repository.GetByIdAsync(id);

			if (entity == null)
			{
				return NotFound();
			}
			var lstUpdateData = !string.IsNullOrEmpty(entity.HistoryUpdate) ? JsonConvert.DeserializeObject<List<Hearing>>(entity.HistoryUpdate) : new List<Hearing>();
			ViewData["HistoryUpdateData"] = lstUpdateData;
			return View(entity);
		}

		//// GET: Hearings/Create
		//public IActionResult Create()
		//{
		//	ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id");
		//	return View();
		//}

		//// POST: Hearings/Create
		//// To protect from overposting attacks, enable the specific properties you want to bind to.
		//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> Create([Bind("Speaker1SPL_1kHz,Id,NUM,Model,CH,Result,DateTime,ProductionLineId,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] Hearing hearing)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		_context.Add(hearing);
		//		await _context.SaveChangesAsync();
		//		return RedirectToAction(nameof(Index));
		//	}
		//	ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", hearing.ProductionLineId);
		//	return View(hearing);
		//}

		//// GET: Hearings/Edit/5
		//public async Task<IActionResult> Edit(long? id)
		//{
		//	if (id == null)
		//	{
		//		return NotFound();
		//	}

		//	var hearing = await _context.Hearings.FindAsync(id);
		//	if (hearing == null)
		//	{
		//		return NotFound();
		//	}
		//	ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", hearing.ProductionLineId);
		//	return View(hearing);
		//}

		//// POST: Hearings/Edit/5
		//// To protect from overposting attacks, enable the specific properties you want to bind to.
		//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> Edit(long id, [Bind("Speaker1SPL_1kHz,Id,NUM,Model,CH,Result,DateTime,ProductionLineId")] Hearing hearing)
		//{
		//	if (id != hearing.Id)
		//	{
		//		return NotFound();
		//	}

		//	if (ModelState.IsValid)
		//	{
		//		try
		//		{
		//			_context.Update(hearing);
		//			await _context.SaveChangesAsync();
		//		}
		//		catch (DbUpdateConcurrencyException)
		//		{
		//			if (!HearingExists(hearing.Id))
		//			{
		//				return NotFound();
		//			}
		//			else
		//			{
		//				throw;
		//			}
		//		}
		//		return RedirectToAction(nameof(Index));
		//	}
		//	ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", hearing.ProductionLineId);
		//	return View(hearing);
		//}

		//// GET: Hearings/Delete/5
		//public async Task<IActionResult> Delete(long? id)
		//{
		//	if (id == null)
		//	{
		//		return NotFound();
		//	}

		//	var hearing = await _context.Hearings
		//		.Include(h => h.CreatedBy)
		//		.Include(h => h.ModifiedBy)
		//		.Include(h => h.ProductionLine)
		//		.FirstOrDefaultAsync(m => m.Id == id);
		//	if (hearing == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(hearing);
		//}

		//// POST: Hearings/Delete/5
		//[HttpPost, ActionName("Delete")]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> DeleteConfirmed(long id)
		//{
		//	var hearing = await _context.Hearings.FindAsync(id);
		//	if (hearing != null)
		//	{
		//		_context.Hearings.Remove(hearing);
		//	}

		//	await _context.SaveChangesAsync();
		//	return RedirectToAction(nameof(Index));
		//}

		//private bool HearingExists(long id)
		//{
		//	return _context.Hearings.Any(e => e.Id == id);
		//}
	}
}
