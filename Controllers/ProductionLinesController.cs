using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Data.Entities;

namespace NFC.Controllers
{
    [Authorize]
    public class ProductionLinesController : Controller
    {
        private readonly NFCDbContext _context;

        public ProductionLinesController(NFCDbContext context)
        {
            _context = context;
        }

        // GET: ProductionLines
        public async Task<IActionResult> Index()
        {
            var nFCDbContext = _context.ProductionLines.Include(p => p.CreatedBy).Include(x => x.ModifiedBy).OrderByDescending(s => s.CreatedOn);
            return View(await nFCDbContext.ToListAsync());
        }

        // GET: ProductionLines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionLine = await _context.ProductionLines
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productionLine == null)
            {
                return NotFound();
            }

            return View(productionLine);
        }

        // GET: ProductionLines/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ProductionLines/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description")] ProductionLine productionLine)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                productionLine.CreatedById = userId;
                productionLine.CreatedOn = DateTime.Now;
                _context.Add(productionLine);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(productionLine);
        }

        // GET: ProductionLines/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionLine = await _context.ProductionLines.FindAsync(id);
            if (productionLine == null)
            {
                return NotFound();
            }
            return View(productionLine);
        }

        // POST: ProductionLines/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] ProductionLine productionLine)
        {
            if (id != productionLine.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var result = await _context.ProductionLines.FindAsync(id);
					result.ModifiedById = userId;
					result.ModifiedOn = DateTime.Now;
                    result.Name = productionLine.Name;
                    result.Description = productionLine.Description;
                    _context.Update(result);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductionLineExists(productionLine.Id))
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
            return View(productionLine);
        }

        // GET: ProductionLines/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productionLine = await _context.ProductionLines
                .Include(p => p.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productionLine == null)
            {
                return NotFound();
            }

            return View(productionLine);
        }

        // POST: ProductionLines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productionLine = await _context.ProductionLines.FindAsync(id);
            if (productionLine != null)
            {
                _context.ProductionLines.Remove(productionLine);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductionLineExists(int id)
        {
            return _context.ProductionLines.Any(e => e.Id == id);
        }
    }
}
