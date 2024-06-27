using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NFC.Data.Entities;
using NFC.Extensions;
using NFC.Models;

namespace NFC.Controllers
{
	[Authorize]
    public class SensorsController(ISensorRepository repository) : Controller
    {
        private readonly ISensorRepository _repository = repository;

		// GET: Sensors
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

		// GET: Sensors/Details/5
		public async Task<IActionResult> Details(int? id)
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
