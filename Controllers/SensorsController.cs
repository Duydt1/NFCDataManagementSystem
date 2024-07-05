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
    public class SensorsController(IServiceProvider serviceProvider) : Controller
	{
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        // GET: Sensors
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
            var repoProductionLine = _serviceProvider.GetService<IProductionLineRepository>();
            var productionLines = await repoProductionLine.GetAllAsync();
            ViewData["ProductionLines"] = new SelectList(productionLines, "Id", "Name");
			if (filterModel.PageNumber < 1) filterModel.PageNumber = 1;

            var repository = _serviceProvider.GetService<ISensorRepository>();
            var results = await repository.GetAllAsync(filterModel);
            GetDayShiftCount(results);
			GetNightShiftCount(results);
			return View(results);
		}
		private void GetDayShiftCount(PaginatedList<Sensor> results)
		{
			var dayShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(8, 0, 0) && x.DateTime.TimeOfDay < new TimeSpan(20, 0, 0));
			var countPass = dayShift.Where(x => x.Result!.Equals("OK", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = dayShift.Where(x => x.Result!.Equals("NG", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = dayShift.Count();
			ViewData["ToTalDayPass"] = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			ViewData["ToTalDayFail"] = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
		}

		private void GetNightShiftCount(PaginatedList<Sensor> results)
		{
			var nightShift = results.Where(x => x.DateTime.TimeOfDay >= new TimeSpan(20, 0, 0) || x.DateTime.TimeOfDay < new TimeSpan(8, 0, 0));
			var countPass = nightShift.Where(x => x.Result!.Equals("OK", StringComparison.CurrentCultureIgnoreCase)).Count();
			var countFail = nightShift.Where(x => x.Result!.Equals("NG", StringComparison.CurrentCultureIgnoreCase)).Count();
			var totalCount = nightShift.Count();
			ViewData["ToTalNightPass"] = totalCount > 0 ? Math.Round((double)countPass / totalCount * 100, 0) : 0;
			ViewData["ToTalNightFail"] = totalCount > 0 ? Math.Round((double)countFail / totalCount * 100, 0) : 0;
		}

		// GET: Sensors/Details/5
		public async Task<IActionResult> Details(long id)
        {
            var repository = _serviceProvider.GetService<ISensorRepository>();
            var entity = await repository.GetByIdAsync(id);
            if (entity == null)
            {
                return NotFound();
            }
			var lstUpdateData = !string.IsNullOrEmpty(entity.HistoryUpdate) ? JsonConvert.DeserializeObject<List<Sensor>>(entity.HistoryUpdate) : new List<Sensor>();
			ViewData["HistoryUpdateData"] = lstUpdateData;
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
