using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NFC.Data;
using NFC.Data.Common;
using NFC.Data.Entities;
using NFC.Models;

namespace NFC.Services
{
	public interface IFileUploadService
	{
		Task ReadFileAndInsertDataAsync(HistoryUpload historyUpload);
		Task<UploadNFCDataResponse> ValidateFileCsvAsync(int type, string uploadFile);
	}
	public class FileUploadService(NFCDbContext context, ILogger<UploadNFCDataJob> logger) : IFileUploadService
	{
		private readonly NFCDbContext _context = context;
		private readonly ILogger<UploadNFCDataJob> _logger = logger;

		public async Task<UploadNFCDataResponse> ValidateFileCsvAsync(int type, string uploadFile)
		{
			var reponse = new UploadNFCDataResponse();
			byte[] bytes = Convert.FromBase64String(uploadFile);
			using var ms = new MemoryStream(bytes);
			using var reader = new StreamReader(ms);
			switch (type)
			{
				case (int)NFCCommon.NFCType.KT_TW_SPL:
					reponse = await ValidateDataKTTW(reader);
					break;
				case (int)NFCCommon.NFCType.KT_MIC_WF_SPL:
					reponse = await ValidateDataKTMIC(reader);
					break;
				case (int)NFCCommon.NFCType.SENSOR:
					reponse = await ValidateDataSensor(reader);
					break;
				case (int)NFCCommon.NFCType.HEARING:
					reponse = await ValidateDataHearing(reader);
					break;
			}
			return reponse;
		}

		public async Task ReadFileAndInsertDataAsync(HistoryUpload historyUpload)
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
				var lstKTTW = await ReadListKTTW(reader);
				if (lstKTTW.Count > 0)
				{
					try
					{
						var existNUMs = lstKTTW.Select(x => x.NUM).ToList();
						var kttws = await _context.KT_TW_SPLs.Where(x => existNUMs.Contains(x.NUM)).ToListAsync();
						foreach (var item in lstKTTW.GroupBy(x => x.NUM).Select(g => g.Last()).ToList())
						{
							var oldEntity = kttws.Where(x => x.NUM == item.NUM).FirstOrDefault();
							if (oldEntity != null)
							{
								string historyUpdate = "";
								if (!string.IsNullOrEmpty(oldEntity.HistoryUpdate))
								{
									var lstUpdateData = JsonConvert.DeserializeObject<List<KT_TW_SPL>>(oldEntity.HistoryUpdate);
									lstUpdateData.Add(oldEntity);
									historyUpdate = JsonConvert.SerializeObject(lstUpdateData, Formatting.Indented);
								}
								else
								{
									var newListOldEntity = new List<KT_TW_SPL>();
									newListOldEntity.Add(oldEntity);
									historyUpdate = JsonConvert.SerializeObject(newListOldEntity, Formatting.Indented);
								}
								oldEntity.HistoryUpdate = historyUpdate;
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
								oldEntity.ProductionLineId = (int)historyUpload.ProductionLineId;
								oldEntity.ModifiedById = historyUpload.CreatedById;
								_context.KT_TW_SPLs.Update(oldEntity);
							}
							else
							{
								item.ProductionLineId = (int)historyUpload.ProductionLineId;
								item.CreatedById = historyUpload.CreatedById;
								item.CreatedOn = DateTime.Now;
								await _context.KT_TW_SPLs.AddAsync(item);
								kttws.Add(item);

                            }
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
				var lstKTMIC = await ReadListKTMIC(reader);
				if (lstKTMIC.Count > 0)
				{
					try
					{
						var existNUMs = lstKTMIC.Select(x => x.NUM).ToList();
						var ktmics = await _context.KT_MIC_WF_SPLs.Where(x => existNUMs.Contains(x.NUM)).ToListAsync();
                        foreach (var item in lstKTMIC.GroupBy(x => x.NUM).Select(g => g.Last()).ToList())
                        {
							var oldEntity = ktmics.Where(x => x.NUM == item.NUM).FirstOrDefault();
							if (oldEntity != null)
							{
								string historyUpdate = "";
								if (!string.IsNullOrEmpty(oldEntity.HistoryUpdate))
								{
									var lstUpdateData = JsonConvert.DeserializeObject<List<KT_MIC_WF_SPL>>(oldEntity.HistoryUpdate);
									lstUpdateData.Add(oldEntity);
									historyUpdate = JsonConvert.SerializeObject(lstUpdateData, Formatting.Indented);
								}
								else
								{
									var newListOldEntity = new List<KT_MIC_WF_SPL>();
									newListOldEntity.Add(oldEntity);
									historyUpdate = JsonConvert.SerializeObject(newListOldEntity, Formatting.Indented);
								}
								oldEntity.HistoryUpdate = historyUpdate;
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
								oldEntity.ProductionLineId = (int)historyUpload.ProductionLineId;
								oldEntity.ModifiedById = historyUpload.CreatedById;
								_context.KT_MIC_WF_SPLs.Update(oldEntity);
							}
							else
							{
								item.CreatedById = historyUpload.CreatedById;
								item.ProductionLineId = (int)historyUpload.ProductionLineId;
								item.CreatedOn = DateTime.Now;
								await _context.KT_MIC_WF_SPLs.AddAsync(item);
                                ktmics.Add(item);
                            }
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
				var lstSensor = await ReadListSensor(reader);
				if (lstSensor.Count > 0)
				{
					try
					{
						var existNUMs = lstSensor.Select(x => x.NUM).ToList();
						var sensors = await _context.Sensors.Where(x => existNUMs.Contains(x.NUM)).ToListAsync();
                        foreach (var item in lstSensor.GroupBy(x => x.NUM).Select(g => g.Last()).ToList())
                        {
							var oldEntity = sensors.Where(x => x.NUM == item.NUM).FirstOrDefault();
							if (oldEntity != null)
							{
								string historyUpdate = "";
								if (!string.IsNullOrEmpty(oldEntity.HistoryUpdate))
								{
									var lstUpdateData = JsonConvert.DeserializeObject<List<Sensor>>(oldEntity.HistoryUpdate);
									lstUpdateData.Add(oldEntity);
									historyUpdate = JsonConvert.SerializeObject(lstUpdateData, Formatting.Indented);
								}
								else
								{
									var newListOldEntity = new List<Sensor>();
									newListOldEntity.Add(oldEntity);
									historyUpdate = JsonConvert.SerializeObject(newListOldEntity, Formatting.Indented);
								}
								oldEntity.HistoryUpdate = historyUpdate;
								oldEntity.NUM = item.NUM;
								oldEntity.CH = item.CH;
								oldEntity.Model = item.Model;
								oldEntity.DateTime = item.DateTime;
								oldEntity.Result = item.Result;
								oldEntity.BattVolt = item.BattVolt;
								oldEntity.DeviceNo = item.DeviceNo;
								oldEntity.ModifiedOn = DateTime.Now;
								oldEntity.ModifiedById = historyUpload.CreatedById;
								oldEntity.ProductionLineId = (int)historyUpload.ProductionLineId;
								_context.Sensors.Update(oldEntity);
							}
							else
							{
								item.CreatedById = historyUpload.CreatedById;
								item.ProductionLineId = (int)historyUpload.ProductionLineId;
								item.CreatedOn = DateTime.Now;
								await _context.Sensors.AddAsync(item);
                                sensors.Add(item);
                            }
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
				var lstHearing = await ReadListHearing(reader);
				if (lstHearing.Count > 0)
				{
					try
					{
						var existNUMs = lstHearing.Select(x => x.NUM).ToList();
						var hearings = await _context.Hearings.Where(x => existNUMs.Contains(x.NUM)).ToListAsync();
                        foreach (var item in lstHearing.GroupBy(x => x.NUM).Select(g => g.Last()).ToList())
                        {
							var oldEntity = hearings.Where(x => x.NUM == item.NUM).FirstOrDefault();
							if (oldEntity != null)
							{
								string historyUpdate = "";
								if (!string.IsNullOrEmpty(oldEntity.HistoryUpdate))
								{
									var lstUpdateData = JsonConvert.DeserializeObject<List<Hearing>>(oldEntity.HistoryUpdate);
									lstUpdateData.Add(oldEntity);
									historyUpdate = JsonConvert.SerializeObject(lstUpdateData, Formatting.Indented);
								}
								else
								{
									var newListOldEntity = new List<Hearing>();
									newListOldEntity.Add(oldEntity);
									historyUpdate = JsonConvert.SerializeObject(newListOldEntity, Formatting.Indented);
								}
								oldEntity.HistoryUpdate = historyUpdate;
								oldEntity.NUM = item.NUM;
								oldEntity.CH = item.CH;
								oldEntity.Model = item.Model;
								oldEntity.DateTime = item.DateTime;
								oldEntity.Result = item.Result;
								oldEntity.Speaker1SPL_1kHz = item.Speaker1SPL_1kHz;
								oldEntity.ModifiedOn = DateTime.Now;
								oldEntity.ProductionLineId = (int)historyUpload.ProductionLineId;
								oldEntity.ModifiedById = historyUpload.CreatedById;
								_context.Hearings.Update(oldEntity);
							}
							else
							{
								item.CreatedById = historyUpload.CreatedById;
								item.ProductionLineId = (int)historyUpload.ProductionLineId;
								item.CreatedOn = DateTime.Now;
								await _context.Hearings.AddAsync(item);
                                hearings.Add(item);
                            }
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

		private async Task<List<KT_TW_SPL>> ReadListKTTW(StreamReader reader)
		{
			string? line;
			var lstKTTW = new List<KT_TW_SPL>();
			while ((line = await reader.ReadLineAsync()) != null)
			{
				if (line.Contains("CH") || string.IsNullOrEmpty(line))
					continue;
				else
				{
					var parts = line.Split(',');
					lstKTTW.Add(new KT_TW_SPL
					{
						NUM = parts[0],
						CH = parts[1],
						Model = parts[2],
						DateTime = !string.IsNullOrEmpty(parts[3]) ? DateTime.Parse(parts[3]) : DateTime.MinValue,
						Grade = parts[5],
						SPL_1kHz = parts[8],
						Polarity = parts[9],
						THD_1kHz = parts[11],
						Impedance_1kHz = parts[13],
						Result = parts[^2],
					});
				}
			}
			return lstKTTW;
		}
		private async Task<List<KT_MIC_WF_SPL>> ReadListKTMIC(StreamReader reader)
		{
			string? line;
			var lstKTMIC = new List<KT_MIC_WF_SPL>();
			while ((line = await reader.ReadLineAsync()) != null)
			{
				if (line.Contains("CH") || string.IsNullOrEmpty(line))
					continue;
				else
				{
					var parts = reader.ReadLine().Split(',');
					lstKTMIC.Add(new KT_MIC_WF_SPL
					{
						NUM = parts[Array.IndexOf(parts, "[ID]") + 1],
						CH = parts[Array.IndexOf(parts, "[CH]") + 1],
						Model = parts[Array.IndexOf(parts, "[MODEL]") + 1],
						DateTime = !string.IsNullOrEmpty(parts[Array.IndexOf(parts, "[TIME]") + 1]) ? DateTime.Parse(parts[Array.IndexOf(parts, "[TIME]") + 1]) : DateTime.MinValue,
						SPL_1kHz = parts[Array.IndexOf(parts, "Speaker1 SPL[1kHz]") + 1],
						Polarity = parts[Array.IndexOf(parts, "Speaker1 Polarity") + 1],
						MIC1Current = parts[Array.IndexOf(parts, "MIC1 Current") + 1],
						MIC1SENS_1kHz = parts[Array.IndexOf(parts, "MIC1 SENS at 1kHz") + 1],
						Impedance_1kHz = parts[Array.IndexOf(parts, "Speaker1 Impedance[1kHz]") + 1],
						SPL_100Hz = parts[Array.IndexOf(parts, "Speaker1 SPL[100Hz]") + 1],
						Result = parts[^2],
					});
				}
			}
			return lstKTMIC;

		}
		private async Task<List<Sensor>> ReadListSensor(StreamReader reader)
		{

			string? line;
			var lstSensor = new List<Sensor>();
			while ((line = await reader.ReadLineAsync()) != null)
			{
				if (line.Contains("CH") || string.IsNullOrEmpty(line))
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
					});
				}
			}
			return lstSensor;


		}
		private async Task<List<Hearing>> ReadListHearing(StreamReader reader)
		{
			string? line;
			var lstHearing = new List<Hearing>();

			while ((line = await reader.ReadLineAsync()) != null)
			{
				if (line.Contains("CH") || string.IsNullOrEmpty(line))
					continue;
				else
				{
					var parts = line.Split(',');
					lstHearing.Add(new Hearing
					{
						NUM = parts[0],
						CH = parts[1],
						Model = parts[2],
						DateTime = !string.IsNullOrEmpty(parts[3]) ? DateTime.Parse(parts[3]) : DateTime.MinValue,
						Speaker1SPL_1kHz = parts[4],
						Result = parts[^2],
					});
				}
			}

			return lstHearing;
		}

		private async Task<UploadNFCDataResponse> ValidateDataKTTW(StreamReader reader)
		{
			var lstKTTW = await ReadListKTTW(reader);
			var result = new UploadNFCDataResponse();
			if (lstKTTW.Count < 1)
				result.Message = "List data is empty";
			else
			{
				var line = 0;
				foreach (var item in lstKTTW)
				{
					line += 1;
					result.Message += string.IsNullOrEmpty(item.NUM) ? $"NUM is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Model) ? $"Model is null; " : "";
					result.Message += string.IsNullOrEmpty(item.CH) ? $"CH is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Result) ? $"Result is null; " : "";
					result.Message += item.DateTime != DateTime.MinValue ? "" : $"Date Time is null; ";
					result.Message += string.IsNullOrEmpty(item.Grade) ? $"Speaker GRADE is null; " : "";
					result.Message += string.IsNullOrEmpty(item.SPL_1kHz) ? $"Speaker1 SPL[1kHz] is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Polarity) ? $"Speaker1 Polarity is null; " : "";
					result.Message += string.IsNullOrEmpty(item.THD_1kHz) ? $"Speaker1 THD[1KHz] is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Impedance_1kHz) ? $"Speaker1 Impedance[1KHz] is null; " : "";
					if (!string.IsNullOrEmpty(result.Message))
					{
						string strNum = $"Error Data in line {line} : ";
						result.Message = strNum + result.Message;
					}
				}
			}

			if (!string.IsNullOrEmpty(result.Message))
				result.Code = System.Net.HttpStatusCode.BadRequest;

			return result;
		}
		private async Task<UploadNFCDataResponse> ValidateDataKTMIC(StreamReader reader)
		{
			var lstKTTW = await ReadListKTMIC(reader);
			var result = new UploadNFCDataResponse();
			if (lstKTTW.Count < 1)
				result.Message = "List data is empty";
			else
			{
				var line = 0;
				foreach (var item in lstKTTW)
				{
					line += 1;
					result.Message += string.IsNullOrEmpty(item.NUM) ? $"NUM is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Model) ? $"Model is null; " : "";
					result.Message += string.IsNullOrEmpty(item.CH) ? $"CH is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Result) ? $"Result is null; " : "";
					result.Message += item.DateTime != DateTime.MinValue ? "" : $"Date Time is null; ";
					result.Message += string.IsNullOrEmpty(item.SPL_100Hz) ? $"Speaker1 SPL[100Hz] is null;" : "";
					result.Message += string.IsNullOrEmpty(item.SPL_1kHz) ? $"Speaker1 SPL[1kHz] is null;" : "";
					result.Message += string.IsNullOrEmpty(item.Polarity) ? $"Speaker1 Polarity is null;" : "";
					result.Message += string.IsNullOrEmpty(item.Impedance_1kHz) ? $"Speaker1 Impedance[1kHz] is null;" : "";
					result.Message += string.IsNullOrEmpty(item.MIC1SENS_1kHz) ? $"MIC1 SENS at 1kHz is null;" : "";
					result.Message += string.IsNullOrEmpty(item.MIC1Current) ? $"MIC1 Current is null;" : "";
					if (!string.IsNullOrEmpty(result.Message))
					{
						string strNum = $"Error Data in line {line} : ";
						result.Message = strNum + result.Message;
					}

				}
			}

			if (!string.IsNullOrEmpty(result.Message))
				result.Code = System.Net.HttpStatusCode.BadRequest;

			return result;
		}
		private async Task<UploadNFCDataResponse> ValidateDataSensor(StreamReader reader)
		{
			var lstKTTW = await ReadListSensor(reader);
			var result = new UploadNFCDataResponse();
			if (lstKTTW.Count < 1)
				result.Message = "List data is empty";
			else
			{
				var line = 0;
				foreach (var item in lstKTTW)
				{
					line += 1;
					result.Message += string.IsNullOrEmpty(item.NUM) ? $"NUM is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Model) ? $"Model is null; " : "";
					result.Message += string.IsNullOrEmpty(item.CH) ? $"CH is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Result) ? $"Result is null; " : "";
					result.Message += item.DateTime != DateTime.MinValue ? "" : $"Date Time is null; ";
					result.Message += string.IsNullOrEmpty(item.DeviceNo) ? $"DEVICE NO is null; " : "";
					result.Message += string.IsNullOrEmpty(item.BattVolt) ? $"BATT. VOLT. is null; " : "";
					if (!string.IsNullOrEmpty(result.Message))
					{
						string strNum = $"Error Data in line {line} : ";
						result.Message = strNum + result.Message;
					}
				}
			}

			if (!string.IsNullOrEmpty(result.Message))
				result.Code = System.Net.HttpStatusCode.BadRequest;

			return result;
		}
		private async Task<UploadNFCDataResponse> ValidateDataHearing(StreamReader reader)
		{
			var lstKTTW = await ReadListHearing(reader);
			var result = new UploadNFCDataResponse();
			if (lstKTTW.Count < 1)
				result.Message = "List data is empty";
			else
			{
				var line = 0;
				foreach (var item in lstKTTW)
				{
					line += 1;
					result.Message += string.IsNullOrEmpty(item.NUM) ? $"NUM is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Model) ? $"Model is null; " : "";
					result.Message += string.IsNullOrEmpty(item.CH) ? $"CH is null; " : "";
					result.Message += string.IsNullOrEmpty(item.Result) ? $"Result is null; " : "";
					result.Message += item.DateTime != DateTime.MinValue ? "" : $"Date Time is null; ";
					result.Message += string.IsNullOrEmpty(item.Speaker1SPL_1kHz) ? $"Speaker1 SPL[1kHz] is null; " : "";
					if (!string.IsNullOrEmpty(result.Message))
					{
						string strNum = $"Error Data in line {line} : ";
						result.Message = strNum + result.Message;
					}
				}
			}
			if (!string.IsNullOrEmpty(result.Message))
				result.Code = System.Net.HttpStatusCode.BadRequest;

			return result;
		}

	}
}
