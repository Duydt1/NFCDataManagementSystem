using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Data.Entities;

namespace NFC.Controllers
{
    [Authorize]
    public class ProductionLinesController(IServiceProvider serviceProvider) : Controller
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        // GET: ProductionLines
        public async Task<IActionResult> Index()
        {
            var repo = _serviceProvider.GetService<IProductionLineRepository>();
            var productionLines = await repo.GetAllAsync();
            return View(productionLines);
        }

        // GET: ProductionLines/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var repo = _serviceProvider.GetService<IProductionLineRepository>();
            var productionLine = await repo.GetByIdAsync(id);
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
                var repo = _serviceProvider.GetService<IProductionLineRepository>();
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                productionLine.CreatedById = userId;
                productionLine.CreatedOn = DateTime.Now;
                await repo.CreateAsync(productionLine);
                return RedirectToAction(nameof(Index));
            }
            return View(productionLine);
        }

        // GET: ProductionLines/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var repo = _serviceProvider.GetService<IProductionLineRepository>();
            var productionLine = await repo.GetByIdAsync(id);
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
                var repo = _serviceProvider.GetService<IProductionLineRepository>();
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await repo.GetByIdAsync(id);
                result.ModifiedById = userId;
                result.ModifiedOn = DateTime.Now;
                result.Name = productionLine.Name;
                result.Description = productionLine.Description;
                await repo.UpdateAsync(result);
                return RedirectToAction(nameof(Index));
            }
            return View(productionLine);
        }

        // GET: ProductionLines/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var repo = _serviceProvider.GetService<IProductionLineRepository>();
            var productionLine = await repo.GetByIdAsync(id);
            if (productionLine == null)
            {
                return NotFound();
            }

            return View(productionLine);
        }

        // POST: ProductionLines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var repo = _serviceProvider.GetService<IProductionLineRepository>();
            await repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
