using Azure;
using Data.Common;
using Data.Models;
using Data.Repositories;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Newtonsoft.Json;
using NFC.Data.Common;
using NFC.Data.Entities;
using NFC.Data.Models;
using NFC.Models;
using NFC.Services;
using System.Net;
using System.Text;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NFC.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UploadDataNFCController(UserManager<NFCUser> userManager,
		IServiceProvider serviceProvider) : ControllerBase
	{
		private readonly UserManager<NFCUser> _userManager = userManager;
		private readonly IServiceProvider _serviceProvider = serviceProvider;

		// GET: api/<ValuesController>
		[HttpGet]
		public string Get()
		{
			return new string("OK");
		}
		[HttpGet("Read")]
		public async Task<string> ReadAsync([FromBody] UploadNFCDataRequest request)
		{
			var fileUploadService = _serviceProvider.GetService<IFileUploadService>();
			byte[] bytes = Convert.FromBase64String(request.NFCContent);
			using var ms = new MemoryStream(bytes);
			using var reader = new StreamReader(ms);
			var lstKTTW = await NFCReadFile.ReadListSensorAsync(reader);
			//var response = await fileUploadService.ValidateFileCsvAsync(request.NFCType, request.NFCContent);
			return new string("OK");
		}

		[HttpGet("Check")]
		public async Task<CheckNumHearingModel> CheckNUMAsync(string num)
		{
			var repository = _serviceProvider.GetService<IHearingRepository>();
			var result = await repository.GetHearingResultAsync(num);
			return result;
		}

		[HttpGet("Reload")]
		public async Task ReloadAsync()
		{
			var repository = _serviceProvider.GetService<IHistoryUploadRepository>();
			var historyUploads = await repository.GetAllFailedAsync();
			if (historyUploads.Count() > 0)
			{
				foreach (var item in historyUploads)
				{
					var nfcDatas = await GetJsonFile(item.Type, item.FileContent);

					var publishEndpoint = _serviceProvider.GetService<IPublishEndpoint>();
					await publishEndpoint.Publish(new MessageUpload
					{
						Id = item.Id,
						Type = item.Type,
						Title = item.Title,
						ProductionLineId = (int)item.ProductionLineId,
						Datas = nfcDatas,
						UserId = item.CreatedById
					});
				}
			}
		}
		private async Task<string> GetJsonFile(int type, string fileContent)
		{
			string datas = "";
			byte[] bytes = Convert.FromBase64String(fileContent);
			using var ms = new MemoryStream(bytes);
			using var reader = new StreamReader(ms);
			switch (type)
			{
				case (int)NFCCommon.NFCType.KT_TW_SPL:
					var lstTW = await NFCReadFile.ReadListKTTWAsync(reader);
					datas = JsonConvert.SerializeObject(lstTW);
					break;
				case (int)NFCCommon.NFCType.KT_MIC_WF_SPL:
					var lstMIC = await NFCReadFile.ReadListKTMICAsync(reader);
					datas = JsonConvert.SerializeObject(lstMIC);
					break;
				case (int)NFCCommon.NFCType.SENSOR:
					var lstSensor = await NFCReadFile.ReadListSensorAsync(reader);
					datas = JsonConvert.SerializeObject(lstSensor);
					break;
				case (int)NFCCommon.NFCType.HEARING:
					var lstHearing = await NFCReadFile.ReadListHearingAsync(reader);
					datas = JsonConvert.SerializeObject(lstHearing);
					break;
			}

			return datas;

		}
		[HttpPost]
		public async Task<UploadNFCDataResponse> Upload([FromBody] UploadNFCDataRequest request)
		{
			var response = new UploadNFCDataResponse();
			var authResult = await AuthenticateAndAuthorizeUserAsync();
			if (!authResult.IsSuccess)
			{
				response.Code = authResult.StatusCode;
				response.Message = authResult.Message;
				return response;
			}
			var user = authResult.User!;
			int productionLineId = user.ProductionLineId;
			if (!Enum.IsDefined(typeof(NFCCommon.NFCType), request.NFCType))
			{
				response.Message = "Invalid NFCType";
				response.Code = HttpStatusCode.NotFound;
				return response;
			}

			var fileUploadService = _serviceProvider.GetRequiredService<IFileUploadService>();
			response = await fileUploadService.ValidateFileCsvAsync(request.NFCType, request.NFCContent);
			if (response.Code == HttpStatusCode.BadRequest)
			{
				return response;
			}

			response.Code = HttpStatusCode.OK;
			response.Message = "File uploaded successfully";

			await CreateHistoryUploadAsync(request, response, productionLineId, user);
			return response;
		}

		private async Task<AuthResult> AuthenticateAndAuthorizeUserAsync()
		{
			var result = new AuthResult();

			// Get the Authorization header
			string authorizationHeader = Request.Headers["Authorization"].FirstOrDefault();
			if (authorizationHeader == null || !authorizationHeader.StartsWith("Basic "))
			{
				result.StatusCode = HttpStatusCode.NotAcceptable;
				result.Message = "Authorization not found or not Basic Auth types";
				return result;
			}

			// Decode the credentials
			string decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(authorizationHeader.Substring("Basic ".Length).Trim()));
			string[] credentialsArray = decodedCredentials.Split(':');
			string username = credentialsArray[0];
			string password = credentialsArray[1];

			// Try to retrieve user from Redis
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var users = await GetUserCacheData();
			
			// Authenticate the user
			var user = users.FirstOrDefault(x => x.UserName == username);

			if (user == null || !await _userManager.CheckPasswordAsync(user, password))
			{
				result.StatusCode = HttpStatusCode.Unauthorized;
				result.Message = "Invalid username or password";
				return result;
			}
			var cacheKey = $"roles";
			var roles = await GetRoleCacheData();

			var role = roles.FirstOrDefault(x => x.Id == user.RoleId);
			if (role == null || (!role.Name!.Contains("Create Data") && !role.Name!.Contains("Admin")))
			{
				result.StatusCode = HttpStatusCode.Forbidden;
				result.Message = "You do not have permission to upload files";
				return result;
			}

			result.IsSuccess = true;
			result.User = user;
			return result;
		}

		private async Task<List<NFCUser>> GetUserCacheData()
		{
			
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			var cacheUserKey = $"users";
			var users = await cache.GetRecordAsync<List<NFCUser>>(cacheUserKey);
			users ??= await repo.GetAllUserAsync();

			await cache.SetRecordAsync(cacheUserKey, users, TimeSpan.FromDays(7));
			return users;
			
		}

		private async Task<List<IdentityRole>> GetRoleCacheData()
		{
			var cache = _serviceProvider.GetService<IDistributedCache>();
			var repo = _serviceProvider.GetService<IIdentityRepository>();
			var cacheKey = $"roles";
			var roles = await cache.GetRecordAsync<List<IdentityRole>>(cacheKey);
			roles ??= await repo.GetAllRolesAsync();

			await cache.SetRecordAsync(cacheKey, roles, TimeSpan.FromDays(7));
			return roles;
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
					Title = response.Title,
					Message = response.Message,
					ProductionLineId = productionLineId != 0 ? productionLineId : null,
					CreatedById = user != null ? user.Id : null,
					CreatedOn = DateTime.Now,
				};
				var repoHistoryUpload = _serviceProvider.GetService<IHistoryUploadRepository>();
				await repoHistoryUpload.CreateAsync(historyUpload);
				await PublishMessageAsync(historyUpload, response);
			}
			catch (Exception ex)
			{
				response.Code = HttpStatusCode.Conflict;
				response.Message = ex.Message;
			}

			return response;
		}
		private async Task PublishMessageAsync(HistoryUpload historyUpload, UploadNFCDataResponse response)
		{
			if (response.Code != HttpStatusCode.OK) return;

			var publishEndpoint = _serviceProvider.GetService<IPublishEndpoint>();
			await publishEndpoint.Publish(new MessageUpload
			{
				Id = historyUpload.Id,
				Type = historyUpload.Type,
				Title = historyUpload.Title,
				ProductionLineId = (int)historyUpload.ProductionLineId,
				Datas = response.NFCDatas,
				UserId = historyUpload.CreatedById
			});
		}

		private class AuthResult
		{
			public bool IsSuccess { get; set; }
			public NFCUser? User { get; set; }
			public HttpStatusCode StatusCode { get; set; }
			public string Message { get; set; } = string.Empty;
		}
	}


}
