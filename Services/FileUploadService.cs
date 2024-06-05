using Microsoft.EntityFrameworkCore;
using NFC.Common;
using NFC.Data;
using NFC.Data.Entities;

namespace NFC.Services
{
	public interface IFileUploadService
	{
		Task ReadFileCsvAsync(HistoryUpload historyUpload);
	}
	public class FileUploadService(NFCDbContext context, ILogger<UploadNFCDataJob> logger) : IFileUploadService
	{
		private readonly NFCDbContext _context = context;
		private readonly ILogger<UploadNFCDataJob> _logger = logger;

		public async Task ReadFileCsvAsync(HistoryUpload historyUpload)
		{
			byte[] bytes = Convert.FromBase64String(historyUpload.FileContent!);
			using var ms = new MemoryStream(bytes);
			using var reader = new StreamReader(ms);
			switch (historyUpload.Type)
			{
				case (int)NFCCommon.NFCType.KT_TW_SPL:
					await InsertListKTTW(reader, historyUpload);
					break;
				case (int)NFCCommon.NFCType.KT_MIC_WF_SPL:
					await InsertListKTMIC(reader, historyUpload);
					break;
				case (int)NFCCommon.NFCType.SENSOR:
					await InsertListSensor(reader, historyUpload);
					break;
				case (int)NFCCommon.NFCType.HEARING:
					await InsertListHearing(reader, historyUpload);
					break;
			}
			if (historyUpload.Status != (int)NFCCommon.HistoryStatus.Declined && historyUpload.Status != (int)NFCCommon.HistoryStatus.Declined)
			{
				historyUpload.Message = "Successfully inserted data!";
				historyUpload.Status = (int)NFCCommon.HistoryStatus.Completed;
			}
			_context.HistoryUploads.Update(historyUpload);
			await _context.SaveChangesAsync();

		}
		private async Task<HistoryUpload> InsertListKTTW(StreamReader reader, HistoryUpload historyUpload)
		{
			try
			{
				string? line;
				var lstKTTW = new List<KT_TW_SPL>();
				while ((line = await reader.ReadLineAsync()) != null)
				{
					if (line.Contains("CH"))
						continue;
					else
					{
						var parts = line.Split(',');
						lstKTTW.Add(new KT_TW_SPL
						{
							NUM = parts[0],
							CH = parts[1],
							Model = parts[2],
							DateTime = DateTime.Parse(parts[3]),
							Grade = parts[5],
							SPL_1kHz = parts[8],
							Polarity = parts[9],
							THD_1kHz = parts[11],
							Impedance_1kHz = parts[13],
							Result = parts[^2],
							ProductionLineId = (int)historyUpload.ProductionLineId,
							CreatedById = historyUpload.CreatedById,
							CreatedOn = DateTime.Now
						});
					}
				}
				if (lstKTTW.Count > 0)
				{
					try
					{
						var kttws = await _context.KT_TW_SPLs.ToListAsync();
						foreach (var item in lstKTTW)
						{
							var oldEntity = kttws.Where(x => x.NUM == item.NUM).FirstOrDefault();
							if (oldEntity != null)
							{
								oldEntity.NUM = item.NUM;
								oldEntity.CH = item.CH;
								oldEntity.Model = item.Model;
								oldEntity.DateTime = item.DateTime;
								oldEntity.SPL_1kHz = item.SPL_1kHz;
								oldEntity.Grade = item.Grade;
								oldEntity.Polarity = item.Polarity;
								oldEntity.THD_1kHz = item.THD_1kHz;
								oldEntity.Impedance_1kHz = item.Impedance_1kHz;
								oldEntity.Result = item.Result;
								oldEntity.ModifiedOn = DateTime.Now;
								oldEntity.ProductionLineId = (int)item.ProductionLineId;
								oldEntity.ModifiedById = item.CreatedById;
								_context.KT_TW_SPLs.Update(oldEntity);
							}
							else
								await _context.KT_TW_SPLs.AddAsync(item);
						}
						await _context.SaveChangesAsync();

					}
					catch (Exception ex)
					{
						historyUpload.Message = ex.Message;
						historyUpload.Status = (int)NFCCommon.HistoryStatus.Pending;
						return historyUpload;
					}

				}
				return historyUpload;
			}
			catch (Exception ex)
			{
				historyUpload.Status = (int)NFCCommon.HistoryStatus.Declined;
				historyUpload.Message = ex.Message;
				_logger.LogError(ex, message: "Error Job UploadNFCDataJob: " + ex.Message);
				return historyUpload;
			}

		}
		private async Task<HistoryUpload> InsertListKTMIC(StreamReader reader, HistoryUpload historyUpload)
		{
			try
			{
				string? line;
				var lstKTMIC = new List<KT_MIC_WF_SPL>();
				while ((line = await reader.ReadLineAsync()) != null)
				{
					var parts = reader.ReadLine().Split(',');
					lstKTMIC.Add(new KT_MIC_WF_SPL
					{
						NUM = parts[Array.IndexOf(parts, "[ID]") + 1],
						CH = parts[Array.IndexOf(parts, "[CH]") + 1],
						Model = parts[Array.IndexOf(parts, "[MODEL]") + 1],
						DateTime = DateTime.Parse(parts[Array.IndexOf(parts, "[TIME]") + 1]),
						SPL_1kHz = parts[Array.IndexOf(parts, "Speaker1 SPL[1kHz]") + 1],
						Polarity = parts[Array.IndexOf(parts, "Speaker1 Polarity") + 1],
						MIC1Current = parts[Array.IndexOf(parts, "MIC1 Current") + 1],
						MIC1SENS_1kHz = parts[Array.IndexOf(parts, "MIC1 SENS at 1kHz") + 1],
						Impedance_1kHz = parts[Array.IndexOf(parts, "Speaker1 Impedance[1kHz]") + 1],
						SPL_100Hz = parts[Array.IndexOf(parts, "Speaker1 SPL[100Hz]") + 1],
						Result = parts[^2],
						CreatedById = historyUpload.CreatedById,
						ProductionLineId = (int)historyUpload.ProductionLineId,
						CreatedOn = DateTime.Now
					});
				}
				if (lstKTMIC.Count > 0)
				{
					try
					{
						var ktmics = await _context.KT_MIC_WF_SPLs.ToListAsync();
						foreach (var item in lstKTMIC)
						{
							var oldEntity = ktmics.Where(x => x.NUM == item.NUM).FirstOrDefault();
							if (oldEntity != null)
							{
								oldEntity.NUM = item.NUM;
								oldEntity.CH = item.CH;
								oldEntity.Model = item.Model;
								oldEntity.DateTime = item.DateTime;
								oldEntity.SPL_1kHz = item.SPL_1kHz;
								oldEntity.SPL_100Hz = item.SPL_100Hz;
								oldEntity.MIC1SENS_1kHz = item.MIC1SENS_1kHz;
								oldEntity.MIC1Current = item.MIC1Current;
								oldEntity.Polarity = item.Polarity;
								oldEntity.Impedance_1kHz = item.Impedance_1kHz;
								oldEntity.Result = item.Result;
								oldEntity.ModifiedOn = DateTime.Now;
								oldEntity.ProductionLineId = item.ProductionLineId;
								oldEntity.ModifiedById = item.CreatedById;
								_context.KT_MIC_WF_SPLs.Update(oldEntity);
							}
							else
								await _context.KT_MIC_WF_SPLs.AddAsync(item);
						}
						await _context.SaveChangesAsync();
					}
					catch (Exception ex)
					{
						historyUpload.Message = ex.Message;
						historyUpload.Status = (int)NFCCommon.HistoryStatus.Pending;
						return historyUpload;
					}

				}
				return historyUpload;
			}
			catch (Exception ex)
			{
				historyUpload.Status = (int)NFCCommon.HistoryStatus.Declined;
				historyUpload.Message = ex.Message;
				_logger.LogError(ex, message: "Error Job UploadNFCDataJob: " + ex.Message);
				return historyUpload;
			}

		}
		private async Task<HistoryUpload> InsertListSensor(StreamReader reader, HistoryUpload historyUpload)
		{
			try
			{
				string? line;
				var lstSensor = new List<Sensor>();
				while ((line = await reader.ReadLineAsync()) != null)
				{
					if (line.Contains("CH"))
						continue;
					else
					{
						var parts = line.Split(',');
						lstSensor.Add(new Sensor
						{
							NUM = parts[0],
							CH = parts[1],
							Model = parts[2],
							DateTime = DateTime.Now,
							Result = parts[^1],
							BattVolt = parts[^3],
							DeviceNo = parts[3],
							CreatedById = historyUpload.CreatedById,
							ProductionLineId = (int)historyUpload.ProductionLineId,
							CreatedOn = DateTime.Now
						});
					}
				}
				if (lstSensor.Count > 0)
				{
					try
					{
						var sensors = await _context.Sensors.ToListAsync();
						foreach (var item in lstSensor)
						{
							var oldEntity = sensors.Where(x => x.NUM == item.NUM).FirstOrDefault();
							if (oldEntity != null)
							{
								oldEntity.NUM = item.NUM;
								oldEntity.CH = item.CH;
								oldEntity.Model = item.Model;
								oldEntity.DateTime = item.DateTime;
								oldEntity.Result = item.Result;
								oldEntity.BattVolt = item.BattVolt;
								oldEntity.DeviceNo = item.DeviceNo;
								oldEntity.ModifiedOn = DateTime.Now;
								oldEntity.ModifiedById = item.CreatedById;
								oldEntity.ProductionLineId = item.ProductionLineId;
								_context.Sensors.Update(oldEntity);
							}
							else
								await _context.Sensors.AddAsync(item);
						}
						await _context.SaveChangesAsync();
					}
					catch (Exception ex)
					{
						historyUpload.Message = ex.Message;
						historyUpload.Status = (int)NFCCommon.HistoryStatus.Pending;
						return historyUpload;
					}


				}
				return historyUpload;
			}
			catch (Exception ex)
			{
				historyUpload.Status = (int)NFCCommon.HistoryStatus.Declined;
				historyUpload.Message = ex.Message;
				_logger.LogError(ex, message: "Error Job UploadNFCDataJob: " + ex.Message);
				return historyUpload;
			}

		}
		private async Task<HistoryUpload> InsertListHearing(StreamReader reader, HistoryUpload historyUpload)
		{
			try
			{
				string? line;
				var lstHearing = new List<Hearing>();

				while ((line = await reader.ReadLineAsync()) != null)
				{
					if (line.Contains("CH"))
						continue;
					else
					{
						var parts = line.Split(',');
						lstHearing.Add(new Hearing
						{
							NUM = parts[0],
							CH = parts[1],
							Model = parts[2],
							DateTime = DateTime.Parse(parts[3]),
							Speaker1SPL_1kHz = parts[4],
							ProductionLineId = (int)historyUpload.ProductionLineId,
							Result = parts[^2],
							CreatedById = historyUpload.CreatedById,
							CreatedOn = DateTime.Now
						});
					}
				}
				if (lstHearing.Count > 0)
				{
					try
					{
						var hearings = await _context.Hearings.ToListAsync();
						foreach (var item in lstHearing)
						{
							var oldEntity = hearings.Where(x => x.NUM == item.NUM).FirstOrDefault();
							if (oldEntity != null)
							{
								oldEntity.NUM = item.NUM;
								oldEntity.CH = item.CH;
								oldEntity.Model = item.Model;
								oldEntity.DateTime = item.DateTime;
								oldEntity.Result = item.Result;
								oldEntity.Speaker1SPL_1kHz = item.Speaker1SPL_1kHz;
								oldEntity.ModifiedOn = DateTime.Now;
								oldEntity.ProductionLineId = item.ProductionLineId;
								oldEntity.ModifiedById = item.CreatedById;
								_context.Hearings.Update(oldEntity);
							}
							else
								await _context.Hearings.AddAsync(item);
						}
						await _context.SaveChangesAsync();
					}
					catch (Exception ex)
					{
						historyUpload.Message = ex.Message;
						historyUpload.Status = (int)NFCCommon.HistoryStatus.Pending;
						return historyUpload;
					}

				}
				return historyUpload;
			}
			catch (Exception ex)
			{
				historyUpload.Status = (int)NFCCommon.HistoryStatus.Declined;
				historyUpload.Message = ex.Message;
				_logger.LogError(ex, message: "Error Job UploadNFCDataJob: " + ex.Message);
				return historyUpload;
			}

		}
	}
}
