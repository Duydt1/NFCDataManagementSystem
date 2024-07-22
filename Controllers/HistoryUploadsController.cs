using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Services;

namespace NFC.Controllers
{
    [Authorize]
    public class HistoryUploadsController(IServiceProvider serviceProvider) : Controller
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        // GET: HistoryUploads
        public async Task<IActionResult> Index()
        {
            var repo = _serviceProvider.GetService<IHistoryUploadRepository>();
            var historyUploads = await repo.GetAllAsync();
			var repoUser = _serviceProvider.GetService<IIdentityRepository>();
			var users = await repoUser.GetAllUserAsync();
			ViewData["Users"] = new SelectList(users, "Id", "FullName");
			return View(historyUploads);
        }

        // GET: HistoryUploads/Details/5
        public async Task<IActionResult> Details(long id)
        {
            var repo = _serviceProvider.GetService<IHistoryUploadRepository>();
            var historyUpload = await repo.GetByIdAsync(id);
            if (historyUpload == null)
            {
                return NotFound();
            }

            return View(historyUpload);
        }

        // GET: HistoryUploads/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HistoryUploads/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Status,Type,FileContent,Message,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] HistoryUpload historyUpload)
        {
            if (ModelState.IsValid)
            {
                var repo = _serviceProvider.GetService<IHistoryUploadRepository>();
                historyUpload.CreatedOn = DateTime.Now;
                await repo.CreateAsync(historyUpload);
                return RedirectToAction(nameof(Index));
            }
            return View(historyUpload);
        }

        // GET: HistoryUploads/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var repo = _serviceProvider.GetService<IHistoryUploadRepository>();
            var historyUpload = await repo.GetByIdAsync(id);
            if (historyUpload == null)
            {
                return NotFound();
            }
            return View(historyUpload);
        }

        // POST: HistoryUploads/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Status,Type,Content,Message,CreatedById,CreatedOn,ModifiedById,ModifiedOn")] HistoryUpload historyUpload)
        {
            if (id != historyUpload.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var repo = _serviceProvider.GetService<IHistoryUploadRepository>();
                historyUpload.ModifiedOn = DateTime.Now;
                await repo.UpdateAsync(historyUpload);
                return RedirectToAction(nameof(Index));
            }
            return View(historyUpload);
        }

        // GET: HistoryUploads/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var repo = _serviceProvider.GetService<IHistoryUploadRepository>();
            var historyUpload = await repo.GetByIdAsync(id);
            if (historyUpload == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Process(long id)
        {
            if (User.IsInRole("Create Data, Super Admin"))
            {
                if (id == null)
                {
                    return NotFound();
                }
                var repo = _serviceProvider.GetService<IHistoryUploadRepository>();
                var historyUpload = await repo.GetByIdAsync(id);
                if (historyUpload != null)
                {
                    try
                    {
						var publishEndpoint = _serviceProvider.GetService<IPublishEndpoint>();
						await publishEndpoint.Publish(historyUpload);
					}
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }
                else
                    return NotFound();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: HistoryUploads/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var repo = _serviceProvider.GetService<IHistoryUploadRepository>();
            repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
