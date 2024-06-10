using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NFC.Common;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Models;
using NuGet.Protocol;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.WebSockets;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NFC.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UploadDataNFCController : ControllerBase
	{
		private readonly UserManager<NFCUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly NFCDbContext _context;

		public UploadDataNFCController(UserManager<NFCUser> userManager, RoleManager<IdentityRole> roleManager,
			NFCDbContext context)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_context = context;
		}

		// GET: api/<ValuesController>
		[HttpGet]
		public IEnumerable<string> Get()
		{
			return new string[] { "He he", "Ha ha" };
		}
		[HttpPost]
		public async Task<UploadNFCDataResponse> Upload([FromBody] UploadNFCDataRequest request)
		{
			var response = new UploadNFCDataResponse();
			int productionLineId = 0;
			var user = new NFCUser();
			// Get the Authorization header
			string authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();

			// Check if the header is in the format "Basic <credentials>"
			if (authorizationHeader != null && authorizationHeader.StartsWith("Basic "))
			{
				// Extract the credentials
				string credentials = authorizationHeader.Substring("Basic ".Length).Trim();

				// Decode the credentials
				string decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(credentials));

				// Split the credentials into username and password
				string[] credentialsArray = decodedCredentials.Split(':');
				string username = credentialsArray[0];
				string password = credentialsArray[1];

				// Authenticate the user
				user = await _userManager.FindByNameAsync(username);
				if (user != null && await _userManager.CheckPasswordAsync(user, password))
				{
					var role = !string.IsNullOrEmpty(user.RoleId) ? await _context.Roles.FindAsync(user.RoleId) : null;
					if (role != null && (role.Name!.Contains("Create Data") || role.Name!.Contains("Admin")))
					{
						response.Message = "File uploaded successfully";
						response.Code = HttpStatusCode.Accepted;
						productionLineId = user.ProductionLineId;
					}
					else
					{
						response.Message = "You do not have permission to upload files";
						response.Code = HttpStatusCode.Forbidden;
					}
				}
				else
				{
					response.Message = "Invalid username or password";
					response.Code = HttpStatusCode.Unauthorized;
				}
			}
			else
			{
				response.Message = "Invalid username or password";
				response.Code = HttpStatusCode.Unauthorized;
			}
			if (!Enum.IsDefined(typeof(NFCCommon.NFCType), request.NFCType))
			{
				response.Message = "Invalid NFCType";
				response.Code = HttpStatusCode.PreconditionFailed;
			}
			await CreateHistoryUploadAsync(request, response, productionLineId, user);
			return response;
		}

		private async Task CreateHistoryUploadAsync(UploadNFCDataRequest request, UploadNFCDataResponse response, int productionLineId, NFCUser? user)
		{
			var historyUpload = new HistoryUpload
			{
				FileContent = request.NFCContent,
				Status = response.Code != HttpStatusCode.Accepted ? (int)NFCCommon.HistoryStatus.Declined : (int)NFCCommon.HistoryStatus.New,
				Type = request.NFCType,
				Message = response.Message,
				ProductionLineId = productionLineId != 0 ? productionLineId : null,
				CreatedById = user != null ? user.Id : null,
				CreatedOn = DateTime.Now,
			};

			await _context.HistoryUploads.AddAsync(historyUpload);
			await _context.SaveChangesAsync();
		}
	}
}
