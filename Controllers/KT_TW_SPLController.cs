﻿using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NFC.Data.Entities;
using NFC.Extensions;
using NFC.Models;

namespace NFC.Controllers
{
	[Authorize]
	public class KT_TW_SPLController(IKT_TW_SPLRepository repository) : Controller
	{
		private readonly IKT_TW_SPLRepository _repository = repository;


		// GET: KT_TW_SPL
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

		// GET: KT_TW_SPL/Details/5
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

            var lstUpdateData = !string.IsNullOrEmpty(entity.HistoryUpdate) ? JsonConvert.DeserializeObject<List<KT_TW_SPL>>(entity.HistoryUpdate) : new List<KT_TW_SPL>();
            ViewData["HistoryUpdateData"] = lstUpdateData;

            return View(entity);
		}

		//// GET: KT_TW_SPL/Create
		//public IActionResult Create()
		//{
		//	ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id");
		//	return View();
		//}

		//// POST: KT_TW_SPL/Create
		//// To protect from overposting attacks, enable the specific properties you want to bind to.
		//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> Create([Bind("Grade,SPL_1kHz,Polarity,THD_1kHz,Impedance_1kHz,Id,NUM,Model,CH,Result,DateTime,ProductionLineId")] KT_TW_SPL kT_TW_SPL)
		//{
		//	if (ModelState.IsValid)
		//	{
		//		_context.Add(kT_TW_SPL);
		//		await _context.SaveChangesAsync();
		//		return RedirectToAction(nameof(Index));
		//	}
		//	ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", kT_TW_SPL.ProductionLineId);
		//	return View(kT_TW_SPL);
		//}

		//// GET: KT_TW_SPL/Edit/5
		//public async Task<IActionResult> Edit(long? id)
		//{
		//	if (id == null)
		//	{
		//		return NotFound();
		//	}

		//	var kT_TW_SPL = await _context.KT_TW_SPLs.FindAsync(id);
		//	if (kT_TW_SPL == null)
		//	{
		//		return NotFound();
		//	}
		//	ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", kT_TW_SPL.ProductionLineId);
		//	return View(kT_TW_SPL);
		//}

		//// POST: KT_TW_SPL/Edit/5
		//// To protect from overposting attacks, enable the specific properties you want to bind to.
		//// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		//[HttpPost]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> Edit(long id, [Bind("Grade,SPL_1kHz,Polarity,THD_1kHz,Impedance_1kHz,Id,NUM,Model,CH,Result,DateTime,ProductionLineId,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] KT_TW_SPL kT_TW_SPL)
		//{
		//	if (id != kT_TW_SPL.Id)
		//	{
		//		return NotFound();
		//	}

		//	if (ModelState.IsValid)
		//	{
		//		try
		//		{
		//			_context.Update(kT_TW_SPL);
		//			await _context.SaveChangesAsync();
		//		}
		//		catch (DbUpdateConcurrencyException)
		//		{
		//			if (!KT_TW_SPLExists(kT_TW_SPL.Id))
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
		//	ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", kT_TW_SPL.ProductionLineId);
		//	return View(kT_TW_SPL);
		//}

		//// GET: KT_TW_SPL/Delete/5
		//public async Task<IActionResult> Delete(long? id)
		//{
		//	if (id == null)
		//	{
		//		return NotFound();
		//	}

		//	var kT_TW_SPL = await _context.KT_TW_SPLs
		//		.Include(k => k.CreatedBy)
		//		.Include(k => k.ModifiedBy)
		//		.Include(k => k.ProductionLine)
		//		.FirstOrDefaultAsync(m => m.Id == id);
		//	if (kT_TW_SPL == null)
		//	{
		//		return NotFound();
		//	}

		//	return View(kT_TW_SPL);
		//}

		//// POST: KT_TW_SPL/Delete/5
		//[HttpPost, ActionName("Delete")]
		//[ValidateAntiForgeryToken]
		//public async Task<IActionResult> DeleteConfirmed(long id)
		//{
		//	var kT_TW_SPL = await _context.KT_TW_SPLs.FindAsync(id);
		//	if (kT_TW_SPL != null)
		//	{
		//		_context.KT_TW_SPLs.Remove(kT_TW_SPL);
		//	}

		//	await _context.SaveChangesAsync();
		//	return RedirectToAction(nameof(Index));
		//}

		//private bool KT_TW_SPLExists(long id)
		//{
		//	return _context.KT_TW_SPLs.Any(e => e.Id == id);
		//}
	}
}
