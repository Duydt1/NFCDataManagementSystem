using Data.Common;
using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using NFC.Models;

namespace NFC.Controllers
{
	[Authorize]
	public class RolesController(IServiceProvider serviceProvider) : Controller
	{
		private readonly IServiceProvider _serviceProvider = serviceProvider;
		public async Task<IActionResult> Index()
		{
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			var cache = _serviceProvider.GetService<IDistributedCache>();

			var cacheKey = $"roles";
			var roles = await cache.GetRecordAsync<List<IdentityRole>>(cacheKey);
			if (roles == null)
			{
				roles = await repo.GetAllRolesAsync();

				await cache.SetRecordAsync(cacheKey, roles, TimeSpan.FromDays(1), TimeSpan.FromHours(1));
			}

			return View(roles);
		}

		public async Task<IActionResult> Details(string? id)
		{
			if (id == null)
			{
				return NotFound();
			}
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			var role = await repo.GetRoleAsync(id);
			if (role == null)
			{
				return NotFound();
			}

			return View(new RoleViewModel { RoleId = role.Id, RoleName = role.Id });
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var repo = _serviceProvider.GetService<IProductionLineRepository>();
			var productionLines = await repo.GetAllAsync();
			ViewData["ProductionLineId"] = new SelectList(productionLines, "Id", "Name");
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
					var roleManager = _serviceProvider.GetService<RoleManager<IdentityRole>>();

					var result = await roleManager.CreateAsync(new IdentityRole
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
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			var result = await repo.GetRoleAsync(id);
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
					var repo = _serviceProvider.GetService<IIdentityRepository>();
					var result = await repo.GetRoleAsync(id);
					result.Name = model.RoleName;
					await repo.UpdateRoleAsync(result);
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
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			var role = await repo.GetRoleAsync(id);
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
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			var role = await repo.GetRoleAsync(id);
			if (role != null)
			{
				await repo.DeleteRoleAsync(role);
			}
			return RedirectToAction(nameof(Index));
		}

		private async Task<bool> RoleExists(string roleName)
		{
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			return await repo.GetRoleExistsAsync(roleName);
		}
	}
}
