using Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            var users = await repo.GetAllUserAsync();
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
            var repoProductionLine = _serviceProvider.GetService<IProductionLineRepository>();
            var productionLines = await repoProductionLine.GetAllAsync();
            var repoRole= _serviceProvider.GetService<IIdentityRepository>();
            var roles = await repoRole.GetAllRolesAsync();
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
                    return RedirectToAction(nameof(Index));
                else return View(model);


            }
            var repoProductionLine = _serviceProvider.GetService<IProductionLineRepository>();
            var productionLines = await repoProductionLine.GetAllAsync();
            var repoRole = _serviceProvider.GetService<IIdentityRepository>();
            var roles = await repoRole.GetAllRolesAsync();
            ViewData["ProductionLineId"] = new SelectList(productionLines, "Id", "Name");
            ViewData["RoleId"] = new SelectList(roles, "Id", "Name");
            return View(model);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var repoProductionLine = _serviceProvider.GetService<IProductionLineRepository>();
            var productionLines = await repoProductionLine.GetAllAsync();
            var repoRole = _serviceProvider.GetService<IIdentityRepository>();
            var roles = await repoRole.GetAllRolesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            var repoProductionLine = _serviceProvider.GetService<IProductionLineRepository>();
            var productionLines = await repoProductionLine.GetAllAsync();
            var repoRole = _serviceProvider.GetService<IIdentityRepository>();
            var roles = await repoRole.GetAllRolesAsync();
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
            return RedirectToAction(nameof(Index));
        }
    }
}
