using ClosedXML.Excel;
using Data.Common;
using Data.Repositories;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using NFC.Data.Entities;
using NFC.Data.Models;
using NFC.Services;
using NuGet.Protocol.Core.Types;
using System.Text;
using static NFC.Data.Common.NFCUtil;
using static System.Formats.Asn1.AsnWriter;

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
			var repoHearing = _serviceProvider.GetService<IHearingRepository>();
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
			var repoHearing = _serviceProvider.GetService<IHearingRepository>();
			var results = await repoHearing.GetAllAsync(filterModel);

			// Generate CSV content
			var csv = new StringBuilder();
			csv.AppendLine("NUM,Model,CH,DateTime,Speaker1SPL_1kHz,Result,ProductionLine,Rub&Buzz Limit,Rub&Buzz Freq Max,Rub&Buzz dB Max");
			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var productionLinesCacheKey = $"productionLines_{userId}";
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
				csv.AppendLine($"{item.NUM},{item.Model},{item.CH},{item.DateTime},{item.Speaker1SPL_1kHz},{item.Result},{productionLineName},{item.Rub_Buzz_Limit},{item.Rub_Buz_FreqMax},{item.Rub_Buz_dBMax}");
			}
			// Return as a CSV file
			return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"HearingsTest{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.csv");
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
			var repoHearing = _serviceProvider.GetService<IHearingRepository>();
			var results = await repoHearing.GetAllAsync(filterModel);

			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var productionLinesCacheKey = $"productionLines_{userId}";
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
				var worksheet = workbook.Worksheets.Add("Hearings Test");

				// Add headers
				worksheet.Cell(1, 1).Value = "Index";
				worksheet.Cell(1, 2).Value = "NUM";
				worksheet.Cell(1, 3).Value = "Model";
				worksheet.Cell(1, 4).Value = "CH";
				worksheet.Cell(1, 5).Value = "DateTime";
				worksheet.Cell(1, 6).Value = "Speaker1SPL_1kHz";
				worksheet.Cell(1, 6).Value = "Speaker1 Rub&Buzz Limit";
				worksheet.Cell(1, 6).Value = "Speaker1 Rub&Buzz[Freq Max]";
				worksheet.Cell(1, 6).Value = "Speaker1 Rub&Buzz[dB Max]";
				worksheet.Cell(1, 7).Value = "Result";
				worksheet.Cell(1, 8).Value = "ProductionLineName";

				// Add data
				for (int i = 0; i < results.Items.Count; i++)
				{
					var item = results.Items[i];
					worksheet.Cell(i + 2, 1).Value = i + 1;
					worksheet.Cell(i + 2, 2).Value = results.Items[i].NUM;
					worksheet.Cell(i + 2, 3).Value = results.Items[i].Model;
					worksheet.Cell(i + 2, 4).Value = results.Items[i].CH;
					worksheet.Cell(i + 2, 5).Value = results.Items[i].DateTime;
					worksheet.Cell(i + 2, 6).Value = results.Items[i].Speaker1SPL_1kHz;
					worksheet.Cell(i + 2, 6).Value = results.Items[i].Rub_Buzz_Limit;
					worksheet.Cell(i + 2, 6).Value = results.Items[i].Rub_Buz_FreqMax;
					worksheet.Cell(i + 2, 6).Value = results.Items[i].Rub_Buz_dBMax;
					worksheet.Cell(i + 2, 7).Value = results.Items[i].Result;
					worksheet.Cell(i + 2, 8).Value = results.Items[i].ProductionLineId != null && dicProductionLine.ContainsKey(results.Items[i].ProductionLineId) ? dicProductionLine[results.Items[i].ProductionLineId] : "Unknown";
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


		private string GetCacheKey(FilterModel filterModel)
		{
			return $"hearings_{filterModel.FromDate?.ToString("yyyy-MM-dd")}_{filterModel.ToDate?.ToString("yyyy-MM-dd")}_{filterModel.ProductionLineId}_{filterModel.PageSize}_{filterModel.PageNumber}";
		}
		private (double TotalDayPass, double TotalDayFail) GetDayShiftCount(PaginatedList<Hearing> results)
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

		private (double TotalNightPass, double TotalNightFail) GetNightShiftCount(PaginatedList<Hearing> results)
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

		public async Task<IActionResult> Details(string num)
		{
			var repository = _serviceProvider.GetService<IHearingRepository>();
			var entity = await repository.GetHearingDetailAsync(num);

			if (entity == null)
			{
				return NotFound();
			}
			var userId = "";
			if (!User.IsInRole("Admin"))
				userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var productionLinesCacheKey = $"productionLines_{userId}";
			var productionLines = await cache.GetRecordAsync<List<ProductionLine>>(productionLinesCacheKey);
			if (productionLines == null)
			{
				var repo = _serviceProvider.GetService<IProductionLineRepository>();
				productionLines = await repo.GetAllAsync(userId);
				await cache.SetRecordAsync(productionLinesCacheKey, productionLines, TimeSpan.FromDays(1));
			}
			ViewData["ProductionLines"] = new SelectList(productionLines, "Id", "Name", 1);
			var lstUpdateData = await repository.GetListByNumAsync(num);
			ViewData["HistoryUpdateData"] = lstUpdateData.Skip(1);
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
