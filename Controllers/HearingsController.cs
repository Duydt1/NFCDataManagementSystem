using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NFC.Data.Entities;
using NFC.Extensions;
using NFC.Models;

namespace NFC.Controllers
{
	[Authorize]
	public class HearingsController(IHearingRepository repository) : Controller
	{
		private readonly IHearingRepository _repository = repository;

		// GET: Hearings
		public async Task<IActionResult> Index(FilterModel filterModel)
		{
			if (!string.IsNullOrEmpty(filterModel.SearchString))
				ViewData["Searching"] = filterModel.SearchString;

			if (!filterModel.FromDate.HasValue)
				filterModel.FromDate = DateTime.Now.AddMonths(-2);
			if (!filterModel.ToDate.HasValue)
				filterModel.ToDate = DateTime.Now;
			ViewData["CurrentFromDate"] = filterModel.FromDate;
			ViewData["CurrentToDate"] = filterModel.ToDate;

			//Sorting
			ViewData["CurrentSort"] = filterModel.SortOrder;
			ViewData["NameSortParm"] = string.IsNullOrEmpty(filterModel.SortOrder) ? "name_desc" : "";
			ViewData["DateSortParm"] = filterModel.SortOrder == "Date" ? "date_desc" : "Date";

			int pageSize = 50;
			if (filterModel.PageNumber < 1) filterModel.PageNumber = 1;

			var results = await _repository.GetAllAsync(filterModel);
			var newDataUploads = results.Where(x => x.CreatedOn.Value >= DateTime.Now.StartOfMonth()).Count();
			var totalCount = results.Count;
			var countPass = results.Where(x => x.Result!.Equals("PASS", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = results.Where(x => x.Result!.Equals("FAIL", StringComparison.CurrentCultureIgnoreCase)).Count();
			ViewData["NewDataUpload"] = newDataUploads;
			ViewData["ToTalPass"] = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			ViewData["ToTalFail"] = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
			return View(results);
		}

		// GET: Hearings/Details/5
		public async Task<IActionResult> Details(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var entity = await _repository.GetByIdAsync((int)id);

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
