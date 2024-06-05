﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using NFC.Common;
using NFC.Data;
using NFC.Data.Entities;
using Quartz;
using System.Buffers.Text;

namespace NFC.Services
{
	public class UploadNFCDataJob(NFCDbContext context, ILogger<UploadNFCDataJob> logger,IFileUploadService fileUploadService) : IJob
	{
		private readonly NFCDbContext _context = context;
		private readonly ILogger<UploadNFCDataJob> _logger = logger;
		private readonly IFileUploadService _fileUploadService = fileUploadService;

		public async Task Execute(IJobExecutionContext context)
		{
			try
			{
				var historyUploadFiles = await _context.HistoryUploads.Where(x => x.Status == (int)NFCCommon.HistoryStatus.New).ToListAsync();
				foreach (var historyUploadFile in historyUploadFiles)
				{
					var isValidBase64 = Base64.IsValid(historyUploadFile.FileContent, out int decodedLength);
					if (!isValidBase64)
					{
						historyUploadFile.Status = (int)NFCCommon.HistoryStatus.Declined;
						historyUploadFile.Message = "File không đúng định dạng base64";
						_context.Update(historyUploadFile);
						await _context.SaveChangesAsync();
						continue;
					}

					historyUploadFile.Status = (int)NFCCommon.HistoryStatus.Processing;
					_context.Update(historyUploadFile);
					await _context.SaveChangesAsync();
					await _fileUploadService.ReadFileCsvAsync(historyUploadFile);
				}
			}
			catch(Exception ex)
			{
				throw new Exception(ex.Message);
			}
			
		}

		
		
	}
}
