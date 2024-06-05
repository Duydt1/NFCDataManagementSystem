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
    public class HearingsController : Controller
    {
        private readonly NFCDbContext _context;

        public HearingsController(NFCDbContext context)
        {
            _context = context;
        }

        // GET: Hearings
        public async Task<IActionResult> Index()
        {
			var results = await _context.Hearings
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

        // GET: Hearings/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hearing = await _context.Hearings
                .Include(h => h.CreatedBy)
                .Include(h => h.ModifiedBy)
                .Include(h => h.ProductionLine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hearing == null)
            {
                return NotFound();
            }

            return View(hearing);
        }

        // GET: Hearings/Create
        public IActionResult Create()
        {
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id");
            return View();
        }

        // POST: Hearings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Speaker1SPL_1kHz,Id,NUM,Model,CH,Result,DateTime,ProductionLineId,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] Hearing hearing)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hearing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", hearing.ProductionLineId);
            return View(hearing);
        }

        // GET: Hearings/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hearing = await _context.Hearings.FindAsync(id);
            if (hearing == null)
            {
                return NotFound();
            }
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", hearing.ProductionLineId);
            return View(hearing);
        }

        // POST: Hearings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Speaker1SPL_1kHz,Id,NUM,Model,CH,Result,DateTime,ProductionLineId")] Hearing hearing)
        {
            if (id != hearing.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hearing);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HearingExists(hearing.Id))
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
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Id", hearing.ProductionLineId);
            return View(hearing);
        }

        // GET: Hearings/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var hearing = await _context.Hearings
                .Include(h => h.CreatedBy)
                .Include(h => h.ModifiedBy)
                .Include(h => h.ProductionLine)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (hearing == null)
            {
                return NotFound();
            }

            return View(hearing);
        }

        // POST: Hearings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var hearing = await _context.Hearings.FindAsync(id);
            if (hearing != null)
            {
                _context.Hearings.Remove(hearing);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool HearingExists(long id)
        {
            return _context.Hearings.Any(e => e.Id == id);
        }
    }
}
