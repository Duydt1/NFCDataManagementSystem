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
    public class SensorsController(IServiceProvider serviceProvider) : Controller
	{
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        // GET: Sensors
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
			var repository = _serviceProvider.GetService<ISensorRepository>();
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
			var repoHearing = _serviceProvider.GetService<ISensorRepository>();
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
			var repoHearing = _serviceProvider.GetService<ISensorRepository>();
			var results = await repoHearing.GetAllAsync(filterModel);

			// Generate CSV content
			var csv = new StringBuilder();
			csv.AppendLine("Index,NUM,Model,CH,DateTime,Device No,J1_5-6[GND-GND](ohm),J1_6-11[GND-GND](ohm),J1_11-17[GND-GND](ohm),J1_17-18[GND-GND](ohm),SHORT(Kohm),SPK TW(ohm),SPK WF(ohm),R2(Kohm),DEVICE ID1,DEVICE ID2,DEVICE ID3,DEVICE ID4,TYPE ID,T0 OPEN,T1 OPEN,T0 KODAK CLOSE,T1 KODAK CLOSE,T0 KODAK DIFF,T1 KODAK DIFF,T0 SKIN CLOSE,T1 SKIN CLOSE,SKIN DIFF,SKIN_RATIO,T0_TRIM_CODE,T1_TRIM_CODE,T0_TRIM_FACTOR,T1_TRIM_FACTOR,CT_ADC_T0_ON,CT_ADC_T0_OFF,CT_ADC_T1_ON,CT_ADC_T1_OFF,CARD_ADC_T0_ON,CARD_ADC_T0_OFF,CARD_ADC_T1_ON,CARD_ADC_T1_OFF,SKIN_ADC_T0_ON,SKIN_ADC_T0_OFF,SKIN_ADC_T1_ON,SKIN_ADC_T1_OFF,TYP_1_T0_G4,TYP_1_T1_G4,TYP_1_T0_TARGET,TYP_1_T1_TARGET,POC_TEMP,ACT_TEMP,MIC 768KHz Peak,Batt. Volt.,Result,Production Line");

			var index = 1;
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
				csv.AppendLine($"{index++},{item.NUM},{item.Model},{item.CH},{item.DateTime},{item.DeviceNo},{item.J1_5},{item.J1_6},{item.J1_11},{item.J1_17},{item.SHORT},{item.SPK_TW},{item.SPK_WF},{item.R2},{item.DEVICE_ID1},{item.DEVICE_ID2},{item.DEVICE_ID3},{item.DEVICE_ID4},{item.TYPE_ID},{item.T0_OPEN},{item.T1_OPEN},{item.T0_KODAK_CLOSE},{item.T1_KODAK_CLOSE},{item.T0_KODAK_DIFF},{item.T1_KODAK_DIFF},{item.T0_SKIN_CLOSE},{item.T1_SKIN_CLOSE},{item.SKIN_DIFF},{item.SKIN_RATIO},{item.T0_TRIM_CODE},{item.T1_TRIM_CODE},{item.T0_TRIM_FACTOR},{item.T1_TRIM_FACTOR},{item.CT_ADC_T0_ON},{item.CT_ADC_T0_OFF},{item.CT_ADC_T1_ON},{item.CT_ADC_T1_OFF},{item.CARD_ADC_T0_ON},{item.CARD_ADC_T0_OFF},{item.CARD_ADC_T1_ON},{item.CARD_ADC_T1_OFF},{item.SKIN_ADC_T0_ON},{item.SKIN_ADC_T0_OFF},{item.SKIN_ADC_T1_ON},{item.SKIN_ADC_T1_OFF},{item.TYP_1_T0_G4},{item.TYP_1_T1_G4},{item.TYP_1_T0_TARGET},{item.TYP_1_T1_TARGET},{item.POC_TEMP},{item.ACT_TEMP},{item.MIC_768KHz_Peak},{item.BattVolt},{item.Result},{productionLineName}");
			}
			return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"SensorsTest{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}.csv");
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
			var repoHearing = _serviceProvider.GetService<ISensorRepository>();
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
				var worksheet = workbook.Worksheets.Add("Sensors Test");

				// Add headers
				worksheet.Cell(1, 1).Value = "Index";
				worksheet.Cell(1, 2).Value = "NUM";
				worksheet.Cell(1, 3).Value = "Model";
				worksheet.Cell(1, 4).Value = "CH";
				worksheet.Cell(1, 5).Value = "DateTime";
				worksheet.Cell(1, 6).Value = "Device No";
				worksheet.Cell(1, 7).Value = "J1_5-6[GND-GND](ohm)";
				worksheet.Cell(1, 8).Value = "J1_6-11[GND-GND](ohm)";
				worksheet.Cell(1, 9).Value = "J1_11-17[GND-GND](ohm)";
				worksheet.Cell(1, 10).Value = "J1_17-18[GND-GND](ohm)";
				worksheet.Cell(1, 11).Value = "SHORT(Kohm)";
				worksheet.Cell(1, 12).Value = "SPK TW(ohm)";
				worksheet.Cell(1, 13).Value = "SPK WF(ohm)";
				worksheet.Cell(1, 14).Value = "R2(Kohm)";
				worksheet.Cell(1, 15).Value = "DEVICE ID1";
				worksheet.Cell(1, 16).Value = "DEVICE ID2";
				worksheet.Cell(1, 17).Value = "DEVICE ID3";
				worksheet.Cell(1, 18).Value = "DEVICE ID4";
				worksheet.Cell(1, 19).Value = "TYPE ID";
				worksheet.Cell(1, 20).Value = "T0 OPEN";
				worksheet.Cell(1, 21).Value = "T1 OPEN";
				worksheet.Cell(1, 22).Value = "T0 KODAK CLOSE";
				worksheet.Cell(1, 23).Value = "T1 KODAK CLOSE";
				worksheet.Cell(1, 24).Value = "T0 KODAK DIFF";
				worksheet.Cell(1, 25).Value = "T1 KODAK DIFF";
				worksheet.Cell(1, 26).Value = "T0 SKIN CLOSE";
				worksheet.Cell(1, 27).Value = "T1 SKIN CLOSE";
				worksheet.Cell(1, 28).Value = "SKIN DIFF";
				worksheet.Cell(1, 29).Value = "SKIN_RATIO";
				worksheet.Cell(1, 30).Value = "T0 TRIM CODE";
				worksheet.Cell(1, 31).Value = "T1 TRIM CODE";
				worksheet.Cell(1, 32).Value = "T0 TRIM FACTOR";
				worksheet.Cell(1, 33).Value = "T1 TRIM FACTOR";
				worksheet.Cell(1, 34).Value = "CT ADC T0 ON";
				worksheet.Cell(1, 35).Value = "CT ADC T0 OFF";
				worksheet.Cell(1, 36).Value = "CT ADC T1 ON";
				worksheet.Cell(1, 37).Value = "CT ADC T1 OFF";
				worksheet.Cell(1, 38).Value = "CARD ADC T0 ON";
				worksheet.Cell(1, 39).Value = "CARD ADC T0 OFF";
				worksheet.Cell(1, 40).Value = "CARD ADC T1 ON";
				worksheet.Cell(1, 41).Value = "CARD ADC T1 OFF";
				worksheet.Cell(1, 42).Value = "SKIN ADC T0 ON";
				worksheet.Cell(1, 43).Value = "SKIN ADC T0 OFF";
				worksheet.Cell(1, 44).Value = "SKIN ADC T1 ON";
				worksheet.Cell(1, 45).Value = "SKIN ADC T1 OFF";
				worksheet.Cell(1, 46).Value = "TYP 1 T0 G4";
				worksheet.Cell(1, 47).Value = "TYP 1 T1 G4";
				worksheet.Cell(1, 48).Value = "TYP 1 T0 TARGET";
				worksheet.Cell(1, 49).Value = "TYP 1 T1 TARGET";
				worksheet.Cell(1, 50).Value = "POC TEMP";
				worksheet.Cell(1, 51).Value = "ACT TEMP";
				worksheet.Cell(1, 52).Value = "MIC 768KHz Peak";
				worksheet.Cell(1, 53).Value = "Batt. Volt.";
				worksheet.Cell(1, 54).Value = "Result";
				worksheet.Cell(1, 55).Value = "Production Line";


				// Add data
				for (int i = 0; i < results.Items.Count; i++)
				{
					worksheet.Cell(i + 2, 1).Value = i + 1;  // Index
					worksheet.Cell(i + 2, 2).Value = results.Items[i].NUM;
					worksheet.Cell(i + 2, 3).Value = results.Items[i].Model;
					worksheet.Cell(i + 2, 4).Value = results.Items[i].CH;
					worksheet.Cell(i + 2, 5).Value = results.Items[i].DateTime;
					worksheet.Cell(i + 2, 6).Value = results.Items[i].DeviceNo;
					worksheet.Cell(i + 2, 7).Value = results.Items[i].J1_5;
					worksheet.Cell(i + 2, 8).Value = results.Items[i].J1_6;
					worksheet.Cell(i + 2, 9).Value = results.Items[i].J1_11;
					worksheet.Cell(i + 2, 10).Value = results.Items[i].J1_17;
					worksheet.Cell(i + 2, 11).Value = results.Items[i].SHORT;
					worksheet.Cell(i + 2, 12).Value = results.Items[i].SPK_TW;
					worksheet.Cell(i + 2, 13).Value = results.Items[i].SPK_WF;
					worksheet.Cell(i + 2, 14).Value = results.Items[i].R2;
					worksheet.Cell(i + 2, 15).Value = results.Items[i].DEVICE_ID1;
					worksheet.Cell(i + 2, 16).Value = results.Items[i].DEVICE_ID2;
					worksheet.Cell(i + 2, 17).Value = results.Items[i].DEVICE_ID3;
					worksheet.Cell(i + 2, 18).Value = results.Items[i].DEVICE_ID4;
					worksheet.Cell(i + 2, 19).Value = results.Items[i].TYPE_ID;
					worksheet.Cell(i + 2, 20).Value = results.Items[i].T0_OPEN;
					worksheet.Cell(i + 2, 21).Value = results.Items[i].T1_OPEN;
					worksheet.Cell(i + 2, 22).Value = results.Items[i].T0_KODAK_CLOSE;
					worksheet.Cell(i + 2, 23).Value = results.Items[i].T1_KODAK_CLOSE;
					worksheet.Cell(i + 2, 24).Value = results.Items[i].T0_KODAK_DIFF;
					worksheet.Cell(i + 2, 25).Value = results.Items[i].T1_KODAK_DIFF;
					worksheet.Cell(i + 2, 26).Value = results.Items[i].T0_SKIN_CLOSE;
					worksheet.Cell(i + 2, 27).Value = results.Items[i].T1_SKIN_CLOSE;
					worksheet.Cell(i + 2, 28).Value = results.Items[i].SKIN_DIFF;
					worksheet.Cell(i + 2, 29).Value = results.Items[i].SKIN_RATIO;
					worksheet.Cell(i + 2, 30).Value = results.Items[i].T0_TRIM_CODE;
					worksheet.Cell(i + 2, 31).Value = results.Items[i].T1_TRIM_CODE;
					worksheet.Cell(i + 2, 32).Value = results.Items[i].T0_TRIM_FACTOR;
					worksheet.Cell(i + 2, 33).Value = results.Items[i].T1_TRIM_FACTOR;
					worksheet.Cell(i + 2, 34).Value = results.Items[i].CT_ADC_T0_ON;
					worksheet.Cell(i + 2, 35).Value = results.Items[i].CT_ADC_T0_OFF;
					worksheet.Cell(i + 2, 36).Value = results.Items[i].CT_ADC_T1_ON;
					worksheet.Cell(i + 2, 37).Value = results.Items[i].CT_ADC_T1_OFF;
					worksheet.Cell(i + 2, 38).Value = results.Items[i].CARD_ADC_T0_ON;
					worksheet.Cell(i + 2, 39).Value = results.Items[i].CARD_ADC_T0_OFF;
					worksheet.Cell(i + 2, 40).Value = results.Items[i].CARD_ADC_T1_ON;
					worksheet.Cell(i + 2, 41).Value = results.Items[i].CARD_ADC_T1_OFF;
					worksheet.Cell(i + 2, 42).Value = results.Items[i].SKIN_ADC_T0_ON;
					worksheet.Cell(i + 2, 43).Value = results.Items[i].SKIN_ADC_T0_OFF;
					worksheet.Cell(i + 2, 44).Value = results.Items[i].SKIN_ADC_T1_ON;
					worksheet.Cell(i + 2, 45).Value = results.Items[i].SKIN_ADC_T1_OFF;
					worksheet.Cell(i + 2, 46).Value = results.Items[i].TYP_1_T0_G4;
					worksheet.Cell(i + 2, 47).Value = results.Items[i].TYP_1_T1_G4;
					worksheet.Cell(i + 2, 48).Value = results.Items[i].TYP_1_T0_TARGET;
					worksheet.Cell(i + 2, 49).Value = results.Items[i].TYP_1_T1_TARGET;
					worksheet.Cell(i + 2, 50).Value = results.Items[i].POC_TEMP;
					worksheet.Cell(i + 2, 51).Value = results.Items[i].ACT_TEMP;
					worksheet.Cell(i + 2, 52).Value = results.Items[i].MIC_768KHz_Peak;
					worksheet.Cell(i + 2, 53).Value = results.Items[i].BattVolt;
					worksheet.Cell(i + 2, 54).Value = results.Items[i].Result;
					worksheet.Cell(i + 2, 55).Value = results.Items[i].ProductionLineId != null && dicProductionLine.ContainsKey(results.Items[i].ProductionLineId) ? dicProductionLine[results.Items[i].ProductionLineId] : "Unknown";
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
			return $"sensors_{filterModel.FromDate?.ToString("yyyy-MM-dd")}_{filterModel.ToDate?.ToString("yyyy-MM-dd")}_{filterModel.ProductionLineId}_{filterModel.PageSize}_{filterModel.PageNumber}";
		}
		private (double TotalDayPass, double TotalDayFail) GetDayShiftCount(PaginatedList<Sensor> results)
		{
			var dayShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(8, 0, 0) && x.DateTime.TimeOfDay < new TimeSpan(20, 0, 0));
			var countPass = dayShift.Where(x => x.Result!.Equals("OK", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = dayShift.Where(x => x.Result!.Equals("NG", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = dayShift.Count();
			var totalDayPass = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			var totalDayFail = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
			ViewData["TotalDayPass"] = totalDayPass;
			ViewData["TotalDayFail"] = totalDayFail;
			return (totalDayPass, totalDayFail);
		}

		private (double TotalNightPass, double TotalNightFail) GetNightShiftCount(PaginatedList<Sensor> results)
		{
			var nightShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(20, 0, 0) || x.DateTime.TimeOfDay < new TimeSpan(8, 0, 0));
			var countPass = nightShift.Where(x => x.Result!.Equals("OK", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = nightShift.Where(x => x.Result!.Equals("NG", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = nightShift.Count();
			var totalNightPass = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			var totalNightFail = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
			ViewData["TotalNightPass"] = totalNightPass;
			ViewData["TotalNightFail"] = totalNightFail;
			return (totalNightPass, totalNightFail);
		}


		// GET: Sensors/Details/5
		public async Task<IActionResult> Details(string num)
        {
            var repository = _serviceProvider.GetService<ISensorRepository>();
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

		

   //     // GET: Sensors/Create
   //     public IActionResult Create()
   //     {
   //         ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id");
   //         return View();
   //     }

   //     // POST: Sensors/Create
   //     // To protect from overposting attacks, enable the specific properties you want to bind to.
   //     // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
   //     [HttpPost]
   //     [ValidateAntiForgeryToken]
   //     public async Task<IActionResult> Create([Bind("DeviceNo,BattVolt,Id,NUM,Model,CH,Result,DateTime,ProductionLineId,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] Sensor sensor)
   //     {
   //         if (ModelState.IsValid)
   //         {
   //             _context.Add(sensor);
   //             await _context.SaveChangesAsync();
   //             return RedirectToAction(nameof(Index));
   //         }
   //         ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", sensor.ProductionLineId);
   //         return View(sensor);
   //     }

   //     // GET: Sensors/Edit/5
   //     public async Task<IActionResult> Edit(long? id)
   //     {
   //         if (id == null)
   //         {
   //             return NotFound();
   //         }

			//var sensor = await _repository.GetByIdAsync((int)id);
			//if (sensor == null)
   //         {
   //             return NotFound();
   //         }
   //         ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", sensor.ProductionLineId);
   //         return View(sensor);
   //     }

   //     // POST: Sensors/Edit/5
   //     // To protect from overposting attacks, enable the specific properties you want to bind to.
   //     // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
   //     [HttpPost]
   //     [ValidateAntiForgeryToken]
   //     public async Task<IActionResult> Edit(long id, [Bind("DeviceNo,BattVolt,Id,NUM,Model,CH,Result,DateTime,ProductionLineId,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] Sensor sensor)
   //     {
   //         if (id != sensor.Id)
   //         {
   //             return NotFound();
   //         }

   //         if (ModelState.IsValid)
   //         {
   //             try
   //             {
   //                 _context.Update(sensor);
   //                 await _context.SaveChangesAsync();
   //             }
   //             catch (DbUpdateConcurrencyException)
   //             {
   //                 if (!SensorExists(sensor.Id))
   //                 {
   //                     return NotFound();
   //                 }
   //                 else
   //                 {
   //                     throw;
   //                 }
   //             }
   //             return RedirectToAction(nameof(Index));
   //         }
   //         ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", sensor.ProductionLineId);
   //         return View(sensor);
   //     }

   //     // GET: Sensors/Delete/5
   //     public async Task<IActionResult> Delete(long? id)
   //     {
   //         if (id == null)
   //         {
   //             return NotFound();
   //         }

   //         var sensor = await _context.Sensors
   //             .Include(s => s.CreatedBy)
   //             .Include(s => s.ModifiedBy)
   //             .Include(s => s.ProductionLine)
   //             .FirstOrDefaultAsync(m => m.Id == id);
   //         if (sensor == null)
   //         {
   //             return NotFound();
   //         }

   //         return View(sensor);
   //     }

   //     // POST: Sensors/Delete/5
   //     [HttpPost, ActionName("Delete")]
   //     [ValidateAntiForgeryToken]
   //     public async Task<IActionResult> DeleteConfirmed(long id)
   //     {
   //         var sensor = await _context.Sensors.FindAsync(id);
   //         if (sensor != null)
   //         {
   //             _context.Sensors.Remove(sensor);
   //         }

   //         await _context.SaveChangesAsync();
   //         return RedirectToAction(nameof(Index));
   //     }

   //     private bool SensorExists(long id)
   //     {
   //         return _context.Sensors.Any(e => e.Id == id);
   //     }
    }
}
