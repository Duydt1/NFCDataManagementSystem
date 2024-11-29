using ClosedXML.Excel;
using Data.Common;
using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NFC.Data.Entities;
using NFC.Data.Models;
using System.Text;
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
                filterModel.FromDate = DateTime.Now.Date.AddDays(-1);

            if (filterModel.ToDate == null)
                filterModel.ToDate = DateTime.Now.Date.AddDays(1);

            ViewData["CurrentFromDate"] = filterModel.FromDate;
			ViewData["CurrentToDate"] = filterModel.ToDate;

			var cache = _serviceProvider.GetService<IDistributedCache>();

			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var productionLinesCacheKey = $"productionLines";
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
			var repository = _serviceProvider.GetService<IKT_MIC_WF_SPLRepository>();
			var results = await repository.GetAllAsync(filterModel);
			GetDayShiftCount(results);
			GetNightShiftCount(results);
			return View(results);
		}
		public async Task<IActionResult> RefreshDataAsync(int productionLineId, DateTime fromDate, DateTime toDate, string searching, int pageSize, int pageNumber)
		{
			var filterModel = new FilterModel
			{
				ProductionLineId = productionLineId,
				FromDate = fromDate,
				SearchString = searching,
				PageNumber = pageNumber,
				PageSize = pageSize,
				ToDate = toDate,
			};
			var repoHearing = _serviceProvider.GetService<IKT_MIC_WF_SPLRepository>();
			var results = await repoHearing.GetAllAsync(filterModel);
			var dayShift = GetDayShiftCount(results);
			var nightShift = GetNightShiftCount(results);
			return Json(new
			{
				items = results.Items,
				pageIndex = results.PageIndex,
				totalPages = results.TotalPages,
				hasPreviousPage = results.HasPreviousPage,
				hasNextPage = results.HasNextPage,
				firstItemIndex = results.FirstItemIndex,
				lastItemIndex = results.LastItemIndex,
				totalItems = results.TotalItems,
				totalDayPass = dayShift.TotalDayPass,
				totalDayFail = dayShift.TotalDayFail,
				totalNightPass = nightShift.TotalNightPass,
				totalNightFail = nightShift.TotalNightFail,
			});
		}
		private string GetCacheKey(FilterModel filterModel)
		{
			return $"ktmics_{filterModel.FromDate?.ToString("yyyy-MM-dd")}_{filterModel.ToDate?.ToString("yyyy-MM-dd")}_{filterModel.ProductionLineId}_{filterModel.PageSize}_{filterModel.PageNumber}";
		}

		public async Task<IActionResult> ExportToCsv(int productionLineId, DateTime fromDate, DateTime toDate, string searching, int pageSize, int pageNumber)
		{
			var filterModel = new FilterModel
			{
				ProductionLineId = productionLineId,
				FromDate = fromDate,
				SearchString = searching,
				PageNumber = pageNumber,
				PageSize = pageSize,
				ToDate = toDate,
			};
			var repoHearing = _serviceProvider.GetService<IKT_MIC_WF_SPLRepository>();
			var results = await repoHearing.GetAllAsync(filterModel);

			// Generate CSV content
			var csv = new StringBuilder();
			csv.AppendLine("Index,NUM,Model,CH,DateTime,FRF Limit,Speaker1 SPL[1kHz],THD Limit,Speaker1 Polarity,Speaker1 Impedance[1kHz],Speaker1 Impedance Limit,MIC1 SENS at 1kHz,MIC1 Current,MIC1 SEQ2 FRF Limit,MIC1 CUR_AVDD,MIC1 CUR_DVDD,Result,ProductionLine");

			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var productionLinesCacheKey = $"productionLines";
			var productionLines = await cache.GetRecordAsync<List<ProductionLine>>(productionLinesCacheKey);
			if (productionLines == null)
			{
				var repo = _serviceProvider.GetService<IProductionLineRepository>();
				productionLines = await repo.GetAllAsync(userId);
				await cache.SetRecordAsync(productionLinesCacheKey, productionLines, TimeSpan.FromDays(1));
			}
			var dicProductionLine = productionLines.ToDictionary(x => x.Id, x => x.Name);

			for (int i = 0; i < results.Items.Count; i++)
			{
				var item = results.Items[i];
				var productionLineName = item.ProductionLineId != null && dicProductionLine.ContainsKey(item.ProductionLineId)
					? dicProductionLine[item.ProductionLineId]
					: "Unknown";

				csv.AppendLine($"{i + 1},{item.NUM},{item.Model},{item.CH},{item.DateTime},{item.FRFLimit},{item.SPL_1kHz},{item.THDLimit},{item.Polarity},{item.Impedance_1kHz},{item.ImpedanceLimit},{item.MIC1SENS_1kHz},{item.MIC1Current},{item.MIC1SEQ2_FRFLimit},{item.MIC1CUR_AVDD},{item.MIC1CUR_DVDD},{item.Result},{productionLineName}");
			}
			// Return as a CSV file
			return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"WF_MIC_SPLTest{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.csv");
		}
		public async Task<IActionResult> ExportToExcel(int productionLineId, DateTime fromDate, DateTime toDate, string searching, int pageSize, int pageNumber)
		{
			var filterModel = new FilterModel
			{
				ProductionLineId = productionLineId,
				FromDate = fromDate,
				SearchString = searching,
				PageNumber = pageNumber,
				PageSize = pageSize,
				ToDate = toDate,
			};
			var repoHearing = _serviceProvider.GetService<IKT_MIC_WF_SPLRepository>();
			var results = await repoHearing.GetAllAsync(filterModel);

			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var productionLinesCacheKey = $"productionLines";
			var productionLines = await cache.GetRecordAsync<List<ProductionLine>>(productionLinesCacheKey);
			if (productionLines == null)
			{
				var repo = _serviceProvider.GetService<IProductionLineRepository>();
				productionLines = await repo.GetAllAsync(userId);
				await cache.SetRecordAsync(productionLinesCacheKey, productionLines, TimeSpan.FromDays(1));
			}
			var dicProductionLine = productionLines.ToDictionary(x => x.Id, x => x.Name);
			// Create a new Excel workbook
			using (var workbook = new XLWorkbook())
			{
				var worksheet = workbook.Worksheets.Add("WF & MIC SPL Test");

				// Add headers
				worksheet.Cell(1, 1).Value = "Index";
				worksheet.Cell(1, 2).Value = "NUM";
				worksheet.Cell(1, 3).Value = "Model";
				worksheet.Cell(1, 4).Value = "CH";
				worksheet.Cell(1, 5).Value = "DateTime";
				worksheet.Cell(1, 6).Value = "FRF Limit";
				worksheet.Cell(1, 7).Value = "Speaker1 SPL[1kHz]";
				worksheet.Cell(1, 8).Value = "THD Limit";
				worksheet.Cell(1, 9).Value = "Speaker1 Polarity";
				worksheet.Cell(1, 10).Value = "Speaker1 Impedance[1kHz]";
				worksheet.Cell(1, 11).Value = "Speaker1 Impedance Limit";
				worksheet.Cell(1, 12).Value = "MIC1 SENS at 1kHz";
				worksheet.Cell(1, 13).Value = "MIC1 Current";
				worksheet.Cell(1, 14).Value = "MIC1 SEQ2 FRF Limit";
				worksheet.Cell(1, 15).Value = "MIC1 CUR_AVDD";
				worksheet.Cell(1, 16).Value = "MIC1 CUR_DVDD";
				worksheet.Cell(1, 17).Value = "Result";
				worksheet.Cell(1, 18).Value = "Production Line";

				// Add data
				for (int i = 0; i < results.Items.Count; i++)
				{
					var item = results.Items[i];
					worksheet.Cell(i + 2, 1).Value = i + 1;  // Index
					worksheet.Cell(i + 2, 2).Value = item.NUM;
					worksheet.Cell(i + 2, 3).Value = item.Model;
					worksheet.Cell(i + 2, 4).Value = item.CH;
					worksheet.Cell(i + 2, 5).Value = item.DateTime;
					worksheet.Cell(i + 2, 6).Value = item.FRFLimit;
					worksheet.Cell(i + 2, 7).Value = item.SPL_1kHz;
					worksheet.Cell(i + 2, 8).Value = item.THDLimit;
					worksheet.Cell(i + 2, 9).Value = item.Polarity;
					worksheet.Cell(i + 2, 10).Value = item.Impedance_1kHz;
					worksheet.Cell(i + 2, 11).Value = item.ImpedanceLimit;
					worksheet.Cell(i + 2, 12).Value = item.MIC1SENS_1kHz;
					worksheet.Cell(i + 2, 13).Value = item.MIC1Current;
					worksheet.Cell(i + 2, 14).Value = item.MIC1SEQ2_FRFLimit;
					worksheet.Cell(i + 2, 15).Value = item.MIC1CUR_AVDD;
					worksheet.Cell(i + 2, 16).Value = item.MIC1CUR_DVDD;
					worksheet.Cell(i + 2, 17).Value = item.Result;
					worksheet.Cell(i + 2, 18).Value = item.ProductionLineId != null && dicProductionLine.ContainsKey(item.ProductionLineId) ? dicProductionLine[item.ProductionLineId] : "Unknown";
				}

				// Prepare the file content
				using (var stream = new MemoryStream())
				{
					workbook.SaveAs(stream);
					var content = stream.ToArray();

					// Return the Excel file
					return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"HearingsData{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.xlsx");
				}
			}
		}

		private (double TotalDayPass, double TotalDayFail) GetDayShiftCount(PaginatedList<KT_MIC_WF_SPL> results)
		{
			var dayShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(8, 0, 0) && x.DateTime.TimeOfDay < new TimeSpan(20, 0, 0));
			var countPass = dayShift.Where(x => x.Result!.Equals("PASS", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = dayShift.Where(x => x.Result!.Equals("FAIL", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = dayShift.Count();
			var totalDayPass = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			var totalDayFail = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
			ViewData["TotalDayPass"] = totalDayPass;
			ViewData["TotalDayFail"] = totalDayFail;
			return (totalDayPass, totalDayFail);
		}

		private (double TotalNightPass, double TotalNightFail) GetNightShiftCount(PaginatedList<KT_MIC_WF_SPL> results)
		{
			var nightShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(20, 0, 0) || x.DateTime.TimeOfDay < new TimeSpan(8, 0, 0));
			var countPass = nightShift.Where(x => x.Result!.Equals("PASS", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = nightShift.Where(x => x.Result!.Equals("FAIL", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = nightShift.Count();
			var totalNightPass = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			var totalNightFail = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
			ViewData["TotalNightPass"] = totalNightPass;
			ViewData["TotalNightFail"] = totalNightFail;
			return (totalNightPass, totalNightFail);
		}
		// GET: KT_MIC_WF_SPL/Details/5
		public async Task<IActionResult> Details(string num)
        {
            var repository = _serviceProvider.GetService<IKT_MIC_WF_SPLRepository>();
            var lst = await repository.GetListByNumAsync(num);

			if (lst.Count == 0)
			{
				return NotFound();
			}
			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var productionLinesCacheKey = $"productionLines";
			var productionLines = await cache.GetRecordAsync<List<ProductionLine>>(productionLinesCacheKey);
			if (productionLines == null)
			{
				var repo = _serviceProvider.GetService<IProductionLineRepository>();
				productionLines = await repo.GetAllAsync(userId);
				await cache.SetRecordAsync(productionLinesCacheKey, productionLines, TimeSpan.FromDays(1));
			}
			var entity = lst.FirstOrDefault();
			ViewData["ProductionLines"] = new SelectList(productionLines, "Id", "Name", 1);
			ViewData["HistoryUpdateData"] = lst.Where(x => x.Id != entity.Id).ToList();

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
