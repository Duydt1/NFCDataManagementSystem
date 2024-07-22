using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using NFC.Data.Entities;
using NFC.Data.Models;
using static NFC.Data.Common.NFCUtil;

namespace NFC.Controllers
{
    [Authorize]
	public class KT_MIC_WF_SPLController(IServiceProvider serviceProvider) : Controller
	{
        private readonly IServiceProvider _serviceProvider = serviceProvider;
		// GET: KT_MIC_WF_SPL
		public async Task<IActionResult> Index(FilterModel filterModel)
		{
			if (!string.IsNullOrEmpty(filterModel.SearchString))
				ViewData["Searching"] = filterModel.SearchString;

            if (filterModel.FromDate == null)
                filterModel.FromDate = DateTime.Now.Date;

            if (filterModel.ToDate == null)
                filterModel.ToDate = DateTime.Now.Date.AddDays(1);

            ViewData["CurrentFromDate"] = filterModel.FromDate;
			ViewData["CurrentToDate"] = filterModel.ToDate;

			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var repoProductionLine = _serviceProvider.GetService<IProductionLineRepository>();
			var productionLines = await repoProductionLine.GetListNameAsync(userId);
			ViewData["ProductionLines"] = new SelectList(productionLines, "Id", "Name", 1);
			ViewBag.PageSize = filterModel.PageSize;
			if (productionLines.Count > 0 && filterModel.ProductionLineId == null)
				filterModel.ProductionLineId = productionLines.FirstOrDefault().Id;

			if (filterModel.PageNumber < 1) filterModel.PageNumber = 1;

            var repository = _serviceProvider.GetService<IKT_MIC_WF_SPLRepository>();
            var results = await repository.GetAllAsync(filterModel);
			GetDayShiftCount(results);
			GetNightShiftCount(results);
			return View(results);
		}
		private void GetDayShiftCount(PaginatedList<KT_MIC_WF_SPL> results)
		{
			var dayShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(8, 0, 0) && x.DateTime.TimeOfDay < new TimeSpan(20, 0, 0));
			var countPass = dayShift.Where(x => x.Result!.Equals("PASS", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = dayShift.Where(x => x.Result!.Equals("FAIL", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = dayShift.Count();
			ViewData["ToTalDayPass"] = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			ViewData["ToTalDayFail"] = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
		}

		private void GetNightShiftCount(PaginatedList<KT_MIC_WF_SPL> results)
		{
			var nightShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(20, 0, 0) || x.DateTime.TimeOfDay < new TimeSpan(8, 0, 0));
			var countPass = nightShift.Where(x => x.Result!.Equals("PASS", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = nightShift.Where(x => x.Result!.Equals("FAIL", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = nightShift.Count();
			ViewData["ToTalNightPass"] = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			ViewData["ToTalNightFail"] = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
		}
		// GET: KT_MIC_WF_SPL/Details/5
		public async Task<IActionResult> Details(long id)
        {
            var repository = _serviceProvider.GetService<IKT_MIC_WF_SPLRepository>();
            var entity = await repository.GetByIdAsync(id);

			if (entity == null)
			{
				return NotFound();
			}
			var lstUpdateData = !string.IsNullOrEmpty(entity.HistoryUpdate) ? JsonConvert.DeserializeObject<List<KT_MIC_WF_SPL>>(entity.HistoryUpdate) : new List<KT_MIC_WF_SPL>();
			ViewData["HistoryUpdateData"] = lstUpdateData;
			return View(entity);
        }

        //// GET: KT_MIC_WF_SPL/Create
        //public IActionResult Create()
        //{
        //    ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id");
        //    return View();
        //}

        //// POST: KT_MIC_WF_SPL/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("SPL_100Hz,SPL_1kHz,Polarity,Impedance_1kHz,MIC1SENS_1kHz,MIC1Current,Id,NUM,Model,CH,Result,DateTime,ProductionLineId")] KT_MIC_WF_SPL kT_MIC_WF_SPL)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(kT_MIC_WF_SPL);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", kT_MIC_WF_SPL.ProductionLineId);
        //    return View(kT_MIC_WF_SPL);
        //}

        //// GET: KT_MIC_WF_SPL/Edit/5
        //public async Task<IActionResult> Edit(long? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var kT_MIC_WF_SPL = await _context.KT_MIC_WF_SPLs.FindAsync(id);
        //    if (kT_MIC_WF_SPL == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", kT_MIC_WF_SPL.ProductionLineId);
        //    return View(kT_MIC_WF_SPL);
        //}

        //// POST: KT_MIC_WF_SPL/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(long id, [Bind("SPL_100Hz,SPL_1kHz,Polarity,Impedance_1kHz,MIC1SENS_1kHz,MIC1Current,Id,NUM,Model,CH,Result,DateTime,ProductionLineId,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] KT_MIC_WF_SPL kT_MIC_WF_SPL)
        //{
        //    if (id != kT_MIC_WF_SPL.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(kT_MIC_WF_SPL);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!KT_MIC_WF_SPLExists(kT_MIC_WF_SPL.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", kT_MIC_WF_SPL.ProductionLineId);
        //    return View(kT_MIC_WF_SPL);
        //}

        //// GET: KT_MIC_WF_SPL/Delete/5
        //public async Task<IActionResult> Delete(long? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var kT_MIC_WF_SPL = await _context.KT_MIC_WF_SPLs
        //        .Include(k => k.CreatedBy)
        //        .Include(k => k.ModifiedBy)
        //        .Include(k => k.ProductionLine)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (kT_MIC_WF_SPL == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(kT_MIC_WF_SPL);
        //}

        //// POST: KT_MIC_WF_SPL/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(long id)
        //{
        //    var kT_MIC_WF_SPL = await _context.KT_MIC_WF_SPLs.FindAsync(id);
        //    if (kT_MIC_WF_SPL != null)
        //    {
        //        _context.KT_MIC_WF_SPLs.Remove(kT_MIC_WF_SPL);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool KT_MIC_WF_SPLExists(long id)
        //{
        //    return _context.KT_MIC_WF_SPLs.Any(e => e.Id == id);
        //}
    }
}
