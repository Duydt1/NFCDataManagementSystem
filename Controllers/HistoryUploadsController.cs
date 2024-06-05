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
using NFC.Services;

namespace NFC.Controllers
{
	[Authorize]
	public class HistoryUploadsController(NFCDbContext context, IFileUploadService fileUploadService) : Controller
	{
		private readonly NFCDbContext _context = context;
		private readonly IFileUploadService _fileUploadService = fileUploadService;

		// GET: HistoryUploads
		public async Task<IActionResult> Index()
		{
			var nFCDbContext = _context.HistoryUploads.Include(h => h.CreatedBy).Include(h => h.ModifiedBy).OrderByDescending(s => s.CreatedOn);
			return View(await nFCDbContext.ToListAsync());
		}

		// GET: HistoryUploads/Details/5
		public async Task<IActionResult> Details(long? id)
		{

			if (id == null)
			{
				return NotFound();
			}

			var historyUpload = await _context.HistoryUploads
				.Include(h => h.CreatedBy)
				.Include(h => h.ModifiedBy)
				.FirstOrDefaultAsync(m => m.Id == id);
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
				historyUpload.CreatedOn = DateTime.Now;
				_context.Add(historyUpload);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(historyUpload);
		}

		// GET: HistoryUploads/Edit/5
		public async Task<IActionResult> Edit(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var historyUpload = await _context.HistoryUploads.FindAsync(id);
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
				try
				{
					historyUpload.ModifiedOn = DateTime.Now;
					_context.Update(historyUpload);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!HistoryUploadExists(historyUpload.Id))
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
			return View(historyUpload);
		}

		// GET: HistoryUploads/Delete/5
		public async Task<IActionResult> Delete(long? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var historyUpload = await _context.HistoryUploads
				.Include(h => h.CreatedBy)
				.Include(h => h.ModifiedBy)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (historyUpload == null)
			{
				return NotFound();
			}

			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Process(long? id)
		{
			if (User.IsInRole("Create Data, Super Admin"))
			{
				if (id == null)
				{
					return NotFound();
				}
				var historyUpload = await _context.HistoryUploads.FirstOrDefaultAsync(x => x.Id == id);
				if (historyUpload != null)
				{
					try
					{
						await _fileUploadService.ReadFileCsvAsync(historyUpload);
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
			var historyUpload = await _context.HistoryUploads.FindAsync(id);
			if (historyUpload != null)
			{
				_context.HistoryUploads.Remove(historyUpload);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool HistoryUploadExists(long id)
		{
			return _context.HistoryUploads.Any(e => e.Id == id);
		}
	}
}
