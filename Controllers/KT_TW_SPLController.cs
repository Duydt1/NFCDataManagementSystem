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
	public class KT_TW_SPLController(IServiceProvider serviceProvider) : Controller
	{
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        // GET: KT_TW_SPL
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

			if (filterModel.PageNumber < 1) filterModel.PageNumber = 1;

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
			var repository = _serviceProvider.GetService<IKT_TW_SPLRepository>();
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
			var repoHearing = _serviceProvider.GetService<IKT_TW_SPLRepository>();
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
				totalDayPass = dayShift.TotalDayPass,
				totalDayFail = dayShift.TotalDayFail,
				totalNightPass = nightShift.TotalNightPass,
				totalNightFail = nightShift.TotalNightFail,
				totalItems = results.TotalItems
			});
		}
		private string GetCacheKey(FilterModel filterModel)
		{
			return $"kttws_{filterModel.FromDate?.ToString("yyyy-MM-dd")}_{filterModel.ToDate?.ToString("yyyy-MM-dd")}_{filterModel.ProductionLineId}_{filterModel.PageSize}_{filterModel.PageNumber}";
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
			var repoHearing = _serviceProvider.GetService<IKT_TW_SPLRepository>();
			var results = await repoHearing.GetAllAsync(filterModel);

			// Generate CSV content
			var csv = new StringBuilder();
			csv.AppendLine("NUM,Model,CH,DateTime,Speaker1FRFLimit,Speaker1SPL_10kHz,Speaker1Polarity,Speaker1THDLimit,Speaker1Impedance_10kHz,Speaker1ImpedanceLimit,MIC1SEQ2_FRFLimit,MIC1SENS_15kHz,Result,ProductionLine");
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
			foreach (var item in results.Items)
			{
				var productionLineName = item.ProductionLineId != null && dicProductionLine.ContainsKey(item.ProductionLineId)
					? dicProductionLine[item.ProductionLineId]
					: "Unknown";
				csv.AppendLine($"{item.NUM},{item.Model},{item.CH},{item.DateTime},{item.FRFLimit},{item.SPL_10kHz},{item.Polarity},{item.THDLimit},{item.Impedance_10kHz},{item.ImpedanceLimit},{item.MIC1SEQ2_FRF_Limit},{item.MIC1SENS_15kHz},{item.Result},{productionLineName}");
			}
			// Return as a CSV file
			return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"TW_MIC_SPLTest{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.csv");
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
			var repoHearing = _serviceProvider.GetService<IKT_TW_SPLRepository>();
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
				var worksheet = workbook.Worksheets.Add("TW & MIC SPL Test Data");

				// Add headers
				worksheet.Cell(1, 1).Value = "NUM";
				worksheet.Cell(1, 2).Value = "Model";
				worksheet.Cell(1, 3).Value = "CH";
				worksheet.Cell(1, 4).Value = "DateTime";
				worksheet.Cell(1, 5).Value = "Speaker1 FRF Limit";
				worksheet.Cell(1, 6).Value = "Speaker1 SPL[10kHz]";
				worksheet.Cell(1, 7).Value = "Speaker1 Polarity";
				worksheet.Cell(1, 8).Value = "Speaker1 THD Limit";
				worksheet.Cell(1, 9).Value = "Speaker1 Impedance[10kHz]";
				worksheet.Cell(1, 10).Value = "Speaker1 Impedance Limit";
				worksheet.Cell(1, 11).Value = "MIC1 SEQ2 FRF Limit";
				worksheet.Cell(1, 12).Value = "MIC1 SENS at 1.5kHz";
				worksheet.Cell(1, 13).Value = "Result";
				worksheet.Cell(1, 14).Value = "ProductionLineName";

				// Add data
				for (int i = 0; i < results.Items.Count; i++)
				{
					worksheet.Cell(i + 2, 1).Value = results.Items[i].NUM;
					worksheet.Cell(i + 2, 2).Value = results.Items[i].Model;
					worksheet.Cell(i + 2, 3).Value = results.Items[i].CH;
					worksheet.Cell(i + 2, 4).Value = results.Items[i].DateTime;
					worksheet.Cell(i + 2, 5).Value = results.Items[i].FRFLimit;
					worksheet.Cell(i + 2, 6).Value = results.Items[i].SPL_10kHz;
					worksheet.Cell(i + 2, 7).Value = results.Items[i].Polarity;
					worksheet.Cell(i + 2, 8).Value = results.Items[i].THDLimit;
					worksheet.Cell(i + 2, 9).Value = results.Items[i].Impedance_10kHz;
					worksheet.Cell(i + 2, 10).Value = results.Items[i].ImpedanceLimit;
					worksheet.Cell(i + 2, 11).Value = results.Items[i].MIC1SEQ2_FRF_Limit;
					worksheet.Cell(i + 2, 12).Value = results.Items[i].MIC1SENS_15kHz;
					worksheet.Cell(i + 2, 13).Value = results.Items[i].Result;
					worksheet.Cell(i + 2, 14).Value = results.Items[i].ProductionLineId != null && dicProductionLine.ContainsKey(results.Items[i].ProductionLineId) ? dicProductionLine[results.Items[i].ProductionLineId] : "Unknown";
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

		private (double TotalDayPass, double TotalDayFail) GetDayShiftCount(PaginatedList<KT_TW_SPL> results)
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

		private (double TotalNightPass, double TotalNightFail) GetNightShiftCount(PaginatedList<KT_TW_SPL> results)
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

		// GET: KT_TW_SPL/Details/5
		public async Task<IActionResult> Details(string num)
		{
            var repository = _serviceProvider.GetService<IKT_TW_SPLRepository>();
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
