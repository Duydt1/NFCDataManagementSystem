using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NFC.Common;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Models;
using NFC.Services;
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
	public class UploadDataNFCController(UserManager<NFCUser> userManager, RoleManager<IdentityRole> roleManager,
		NFCDbContext context, IFileUploadService fileUploadService) : ControllerBase
	{
		private readonly UserManager<NFCUser> _userManager = userManager;
		private readonly RoleManager<IdentityRole> _roleManager = roleManager;
		private readonly NFCDbContext _context = context;
		private readonly IFileUploadService _fileUploadService = fileUploadService;

		// GET: api/<ValuesController>
		[HttpGet]
		public string Get()
		{
			return new string("OK");
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
					if (role == null || (!role.Name!.Contains("Create Data") && !role.Name!.Contains("Admin")))
					{
						response.Message = "You do not have permission to upload files";
						response.Code = HttpStatusCode.Forbidden;
					}
					else
					{
                        if (!Enum.IsDefined(typeof(NFCCommon.NFCType), request.NFCType))
                        {
                            response.Message = "Invalid NFCType";
                            response.Code = HttpStatusCode.NotFound;
						}
						else
						{
                            response = await _fileUploadService.ValidateFileCsvAsync(request.NFCType, request.NFCContent);
                            if (response.Code != HttpStatusCode.BadRequest)
                            {
                                response.Code = HttpStatusCode.OK;
                                response.Message = "File uploaded successfully";
                            }
                        }
						productionLineId = user.ProductionLineId;
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
				response.Message = "Authorization not found or not Basic Auth types";
				response.Code = HttpStatusCode.NotAcceptable;
			}
			await CreateHistoryUploadAsync(request, response, productionLineId, user);
			return response;
		}

		private async Task<UploadNFCDataResponse> CreateHistoryUploadAsync(UploadNFCDataRequest request, UploadNFCDataResponse response, int productionLineId, NFCUser? user)
		{
			try
			{
				var historyUpload = new HistoryUpload
				{
					FileContent = request.NFCContent,
					Status = response.Code != HttpStatusCode.OK ? (int)NFCCommon.HistoryStatus.Declined : (int)NFCCommon.HistoryStatus.New,
					Type = request.NFCType,
					Message = response.Message,
					ProductionLineId = productionLineId != 0 ? productionLineId : null,
					CreatedById = user != null ? user.Id : null,
					CreatedOn = DateTime.Now,
				};

				await _context.HistoryUploads.AddAsync(historyUpload);
				await _context.SaveChangesAsync();
			}
			catch(Exception ex)
			{
				response.Code = HttpStatusCode.Conflict;
				response.Message = ex.Message;
			}
			
			return response;
		}
	}
}
