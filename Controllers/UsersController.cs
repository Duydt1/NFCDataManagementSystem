using Data.Common;
using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Distributed;
using NFC.Data.Entities;
using NFC.Data.Models;

namespace NFC.Controllers
{
	[Authorize]
    public class UsersController(IServiceProvider serviceProvider) : Controller
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task<IActionResult> Index()
        {
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			var cache = _serviceProvider.GetService<IDistributedCache>();

			var cacheKey = $"users";
			var users = await cache.GetRecordAsync<List<NFCUser>>(cacheKey);
			if (users == null)
			{
				users = await repo.GetAllUserAsync();

				await cache.SetRecordAsync(cacheKey, users, TimeSpan.FromDays(7));
			}

			return View(users);
		}

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var repo = _serviceProvider.GetService<IIdentityRepository>();
            var nfcUser = await repo.GetUserAsync(id);
            if (nfcUser == null)
            {
                return NotFound();
            }

            return View(new UserViewModel
            {
                FirstName = nfcUser.FirstName,
                MiddleName = nfcUser.MiddleName,
                LastName = nfcUser.LastName,
                UserName = nfcUser.UserName,
                Address = nfcUser.Address,
                Email = nfcUser.Email,
                RoleId = nfcUser.RoleId,
                Code = nfcUser.Code,
                ProductionLineId = nfcUser.ProductionLineId,
                PhoneNumber = nfcUser.PhoneNumber,
                Gender = nfcUser.Gender,
                DateOfBirth = nfcUser.DateOfBirth,
                Password = nfcUser.PasswordHash,
            });
        }
        [HttpGet]
        public async Task<IActionResult> CreateAsync()
        {
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var productionLinesCacheKey = $"productionLines";
			var productionLines = await cache.GetRecordAsync<List<ProductionLine>>(productionLinesCacheKey);
			var roles = await cache.GetRecordAsync<List<IdentityRole>>("roles");
			ViewData["ProductionLineId"] = new SelectList(productionLines, "Id", "Name");
            ViewData["RoleId"] = new SelectList(roles, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var repo = _serviceProvider.GetService<IIdentityRepository>();
                var user = new NFCUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    DateOfBirth = model.DateOfBirth,
                    Code = model.Code,
                    Gender = model.Gender,
                    ProductionLineId = model.ProductionLineId,
                    RoleId = model.RoleId,
                    Address = model.Address,
                    EmailConfirmed = true
                };
                var result = await repo.CreateUserAsync(user, model.Password);
                if (result.Id != null)
                {
					await RefreshRolesCache();
					return RedirectToAction(nameof(Index));
				}
				else return View(model);


            }
            return View(model);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
			var productionLinesCacheKey = $"productionLines";
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var productionLines = await cache.GetRecordAsync<List<ProductionLine>>(productionLinesCacheKey);
			var roles = await cache.GetRecordAsync<List<IdentityRole>>("roles");
			ViewData["ProductionLineId"] = new SelectList(productionLines, "Id", "Name");
            ViewData["RoleId"] = new SelectList(roles, "Id", "Name");
            var repo = _serviceProvider.GetService<IIdentityRepository>();
            var nfcUser = await repo.GetUserAsync(id);
            if (nfcUser == null)
            {
                return NotFound();
            }
            return View(new UserViewModel
            {
                FirstName = nfcUser.FirstName,
                MiddleName = nfcUser.MiddleName,
                LastName = nfcUser.LastName,
                UserName = nfcUser.UserName,
                Address = nfcUser.Address,
                Email = nfcUser.Email,
                RoleId = nfcUser.RoleId,
                Code = nfcUser.Code,
                ProductionLineId = nfcUser.ProductionLineId,
                PhoneNumber = nfcUser.PhoneNumber,
                Gender = nfcUser.Gender,
                DateOfBirth = nfcUser.DateOfBirth,
                Password = nfcUser.PasswordHash,
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var repo = _serviceProvider.GetService<IIdentityRepository>();
                    var user = await repo.GetUserAsync(id);
                    if (user == null)
                    {
                        return NotFound();
                    }
                    user.FirstName = model.FirstName;
                    user.MiddleName = model.MiddleName;
                    user.LastName = model.LastName;
                    user.UserName = model.UserName;
                    user.Address = model.Address;
                    user.Code = model.Code;
                    user.Gender = model.Gender;
                    user.Email = model.Email;
                    user.RoleId = model.RoleId;
                    user.DateOfBirth = model.DateOfBirth;
                    user.ProductionLineId = model.ProductionLineId;
                    user.PhoneNumber = model.PhoneNumber;
                    var repoUser = _serviceProvider.GetService<IIdentityRepository>();
                    await repoUser.UpdateUserAsync(user, model.NewPassword, model.CurrentPassword);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                await RefreshRolesCache();
				return RedirectToAction(nameof(Index));
            }
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var productionLinesCacheKey = $"productionLines";
			var productionLines = await cache.GetRecordAsync<List<ProductionLine>>(productionLinesCacheKey);
			var roles = await cache.GetRecordAsync<List<IdentityRole>>("roles");
			ViewData["ProductionLineId"] = new SelectList(productionLines, "Id", "Name");
            ViewData["RoleId"] = new SelectList(roles, "Id", "Name");
            return View(model);
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var repo = _serviceProvider.GetService<IIdentityRepository>();
            var nfcUser = await repo.GetUserAsync(id);
            if (nfcUser == null)
            {
                return NotFound();
            }

            return View(new UserViewModel
            {
                FirstName = nfcUser.FirstName,
                MiddleName = nfcUser.MiddleName,
                LastName = nfcUser.LastName,
                UserName = nfcUser.UserName,
                Address = nfcUser.Address,
                Email = nfcUser.Email,
                RoleId = nfcUser.RoleId,
                Code = nfcUser.Code,
                ProductionLineId = nfcUser.ProductionLineId,
                PhoneNumber = nfcUser.PhoneNumber,
                Gender = nfcUser.Gender,
                DateOfBirth = nfcUser.DateOfBirth,
                Password = nfcUser.PasswordHash,
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var repoUser = _serviceProvider.GetService<IIdentityRepository>();
            var nfcUser = await repoUser.GetUserAsync(id);
            if (nfcUser != null)
            {
                await repoUser.DeleteUserAsync(nfcUser);
			}
			await RefreshRolesCache();
			return RedirectToAction(nameof(Index));
        }

		private async Task RefreshRolesCache()
		{
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			var cache = _serviceProvider.GetService<IDistributedCache>();

			var cacheKey = $"users";
			var users = await repo.GetAllUserAsync();

			await cache.SetRecordAsync(cacheKey, users, TimeSpan.FromDays(7));
		}
	}
}
