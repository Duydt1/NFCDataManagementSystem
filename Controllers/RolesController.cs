using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NFC.Data.Entities;
using NFC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using NFC.Models;
using System.Net.WebSockets;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace NFC.Controllers
{
	[Authorize]
	public class RolesController(
		RoleManager<IdentityRole> roleManager,
		SignInManager<NFCUser> signInManager,
		UserManager<NFCUser> userManager,
		NFCDbContext context) : Controller
	{
		private readonly RoleManager<IdentityRole> _roleManager = roleManager;
		private readonly SignInManager<NFCUser> _signInManager = signInManager;
		private readonly NFCDbContext _context = context;
		private readonly UserManager<NFCUser> _userManager = userManager;

		public async Task<IActionResult> Index()
		{
			var roles = await _context.Roles.ToListAsync();
			return View(roles);
		}

		public async Task<IActionResult> Details(string? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var role = await _context.Roles
				.FirstOrDefaultAsync(m => m.Id == id);
			if (role == null)
			{
				return NotFound();
			}

			return View(new RoleViewModel { RoleId = role.Id, RoleName = role.Id });
		}

		[HttpGet]
		public IActionResult Create()
		{
			ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Name");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(RoleViewModel model)
		{
			if (ModelState.IsValid)
			{
				if (!await RoleExists(model.RoleName))
				{
					var result = await _roleManager.CreateAsync(new IdentityRole
					{
						Name = model.RoleName
					});
					if (result.Succeeded)
						return RedirectToAction(nameof(Index));
					else return View(result);
				}
			}
			return View(model);
		}

		public async Task<IActionResult> Edit(string? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var role = new RoleViewModel();
			var result = await _context.Roles.FindAsync(id);
			if (result == null)
			{
				return NotFound();
			}
			role.RoleName = result.Name;
			role.RoleId = result.Id;
			return View(role);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, RoleViewModel model)
		{
			if (id != model.RoleId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				if (!await RoleExists(model.RoleName))
				{
					var result = await _context.Roles.FindAsync(id);
					result.Name = model.RoleName;
					_context.Update(result);
					await _context.SaveChangesAsync();
				}
				return RedirectToAction(nameof(Index));
			}
			return View(model);
		}

		public async Task<IActionResult> Delete(string? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var role = await _context.Roles
				.FirstOrDefaultAsync(m => m.Id == id);
			if (role == null)
			{
				return NotFound();
			}

			return View(new RoleViewModel { RoleName = role.Name, RoleId = role.Id });
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(string id)
		{
			var role = await _context.Roles.FindAsync(id);
			if (role != null)
			{
				_context.Roles.Remove(role);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private async Task<bool> RoleExists(string roleName)
		{
			return await _roleManager.RoleExistsAsync(roleName);
		}
	}
}
