using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace NFC.Services
{
	public class CustomUserManager : UserManager<IdentityUser>
	{
		private readonly RoleManager<IdentityRole> _roleManager;

		public CustomUserManager(UserStore<IdentityUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<IdentityUser> passwordHasher, IEnumerable<IUserValidator<IdentityUser>> userValidators, IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<IdentityUser>> logger, RoleManager<IdentityRole> roleManager)
			: base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
		{
			_roleManager = roleManager;
		}

		public override async Task<IdentityResult> CreateAsync(IdentityUser user, string password)
		{
			var result = await base.CreateAsync(user, password);
			if (result.Succeeded)
			{
				var adminRole = await _roleManager.FindByNameAsync("Admin");
				if (adminRole != null)
				{
					await AddToRoleAsync(user, adminRole.Name);
				}
			}
			return result;
		}
	}
}
