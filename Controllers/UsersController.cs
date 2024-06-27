using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Models;
using System.Security.Claims;

namespace NFC.Controllers
{
	[Authorize]
    public class UsersController(UserManager<NFCUser> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<NFCUser> signInManager,
        NFCDbContext context) : Controller
    {
        private readonly UserManager<NFCUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly SignInManager<NFCUser> _signInManager = signInManager;
        private readonly NFCDbContext _context = context;

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.Include(x => x.Role).ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nfcUser = await _context.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
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
        public IActionResult Create()
        {
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Name");
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
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
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == model.RoleId);
                    await _userManager.AddToRoleAsync(user, role.Name);
                    return RedirectToAction(nameof(Index));

                }
                else return View(model);


            }
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Name", model.ProductionLineId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", model.RoleId);
            return View(model);
        }

        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Name");
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name");
            var nfcUser = await _context.Users.FindAsync(id);
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
                    var user = await _context.Users.FindAsync(id);
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
                    _context.Update(user);
                    if (!string.IsNullOrEmpty(model.NewPassword) && !string.IsNullOrEmpty(model.CurrentPassword))
                    {

                        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                        if (result.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: false);
                            return RedirectToAction(nameof(Index));
                        }
                    }
                    var role = await _context.Roles.FirstOrDefaultAsync(x => x.Id == model.RoleId);
                    await _userManager.AddToRoleAsync(user, role.Name);
                    // Add the role claim to the user's identity
                    await _userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, role.Name));

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NFCUserExists(model.Id))
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
            ViewData["ProductionLineId"] = new SelectList(_context.ProductionLines, "Id", "Name", model.ProductionLineId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "Id", "Name", model.RoleId);
            return View(model);
        }

        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nfcUser = await _context.Users.Include(x => x.Role)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var nfcUser = await _context.Users.FindAsync(id);
            if (nfcUser != null)
            {
                _context.Users.Remove(nfcUser);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NFCUserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
