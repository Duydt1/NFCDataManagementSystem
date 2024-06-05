using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Extensions;

namespace NFC.Controllers
{
    [Authorize]
    public class SensorsController : Controller
    {
        private readonly NFCDbContext _context;

        public SensorsController(NFCDbContext context)
        {
            _context = context;
        }

        // GET: Sensors
        public async Task<IActionResult> Index()
        {
			var results = await _context.Sensors
				.Include(k => k.CreatedBy)
				.Include(k => k.ProductionLine)
				.OrderByDescending(s => s.DateTime)
				.ToListAsync();
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
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensor = await _context.Sensors
                .Include(s => s.CreatedBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.ProductionLine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sensor == null)
            {
                return NotFound();
            }

            return View(sensor);
        }

        // GET: Sensors/Create
        public IActionResult Create()
        {
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id");
            return View();
        }

        // POST: Sensors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DeviceNo,BattVolt,Id,NUM,Model,CH,Result,DateTime,ProductionLineId,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] Sensor sensor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(sensor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", sensor.ProductionLineId);
            return View(sensor);
        }

        // GET: Sensors/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor == null)
            {
                return NotFound();
            }
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", sensor.ProductionLineId);
            return View(sensor);
        }

        // POST: Sensors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("DeviceNo,BattVolt,Id,NUM,Model,CH,Result,DateTime,ProductionLineId,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] Sensor sensor)
        {
            if (id != sensor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sensor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SensorExists(sensor.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", sensor.ProductionLineId);
            return View(sensor);
        }

        // GET: Sensors/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensor = await _context.Sensors
                .Include(s => s.CreatedBy)
                .Include(s => s.ModifiedBy)
                .Include(s => s.ProductionLine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sensor == null)
            {
                return NotFound();
            }

            return View(sensor);
        }

        // POST: Sensors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var sensor = await _context.Sensors.FindAsync(id);
            if (sensor != null)
            {
                _context.Sensors.Remove(sensor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SensorExists(long id)
        {
            return _context.Sensors.Any(e => e.Id == id);
        }
    }
}
