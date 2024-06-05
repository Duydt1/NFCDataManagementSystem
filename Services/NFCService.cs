using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Models;

namespace NFC.Services
{
	public interface INFCService
	{
		Task<List<NFCModel>> GetNFCDashboard(string? userId, int? productionLineId);
	}
	public class NFCService(NFCDbContext context) : INFCService
	{
		private readonly NFCDbContext _context = context;

		public async Task<List<NFCModel>> GetNFCDashboard(string? userId, int? productionLineId)
		{
			var nfcModels = new List<NFCModel>();
			var lstTW = await GetListKTTW(userId, productionLineId);
			var lstMIC = await GetListKTMIC(userId, productionLineId);
			var lstSensor = await GetListSensor(userId, productionLineId);
			var lstHearing = await GetListHearing(userId, productionLineId);
			foreach(var item in lstTW)
			{
				nfcModels.Add(new NFCModel
				{
					CH = item.CH,
					NUM = item.NUM,
					Model = item.Model,
					DateTime = item.DateTime,
					Hearing = lstHearing.FirstOrDefault(x => x.NUM == item.NUM),
					KT_TW_SPL = lstTW.FirstOrDefault(x => x.NUM == item.NUM),
					KT_MIC_WF_SPL = lstMIC.FirstOrDefault(x => x.NUM == item.NUM),
					Sensor = lstSensor.FirstOrDefault(x => x.NUM == item.NUM),
				});
			}
			return nfcModels;
		}

		private async Task<List<KT_TW_SPL>> GetListKTTW(string? userId, int? productionLineId)
		{
			var result = new List<KT_TW_SPL>();
			result = await _context.KT_TW_SPLs.Select(x => new KT_TW_SPL
			{
				CH = x.CH,
				NUM = x.NUM,
				Model = x.Model,
				Polarity = x.Polarity,
				Grade = x.Grade,
				THD_1kHz = x.THD_1kHz,
				SPL_1kHz = x.SPL_1kHz,
				Impedance_1kHz = x.Impedance_1kHz,
				Result = x.Result,
				DateTime = x.DateTime,
				CreatedById = x.CreatedById,
				ProductionLineId = x.ProductionLineId
			}).Where(x => string.IsNullOrEmpty(userId) || x.CreatedById == userId)
			.Where(x => productionLineId != null || x.ProductionLineId == productionLineId)
			.ToListAsync();
			return result;
		}
		private async Task<List<KT_MIC_WF_SPL>> GetListKTMIC(string? userId, int? productionLineId)
		{
			var result = new List<KT_MIC_WF_SPL>();
			result = await _context.KT_MIC_WF_SPLs.Select(x => new KT_MIC_WF_SPL
			{
				CH = x.CH,
				NUM = x.NUM,
				Model = x.Model,
				Polarity = x.Polarity,
				SPL_1kHz = x.SPL_1kHz,
				SPL_100Hz = x.SPL_100Hz,
				Impedance_1kHz = x.Impedance_1kHz,
				MIC1SENS_1kHz = x.MIC1SENS_1kHz,
				MIC1Current = x.MIC1Current,
				Result = x.Result,
				DateTime = x.DateTime,
				CreatedById = x.CreatedById,
				ProductionLineId = x.ProductionLineId
			}).Where(x => string.IsNullOrEmpty(userId) || x.CreatedById == userId)
			.Where(x => productionLineId != null || x.ProductionLineId == productionLineId)
			.ToListAsync();
			return result;
		}
		private async Task<List<Sensor>> GetListSensor(string? userId, int? productionLineId)
		{
			var result = new List<Sensor>();
			result = await _context.Sensors.Select(x => new Sensor
			{
				CH = x.CH,
				NUM = x.NUM,
				Model = x.Model,
				BattVolt = x.BattVolt,
				DeviceNo = x.DeviceNo,
				Result = x.Result,
				DateTime = x.DateTime,
				CreatedById = x.CreatedById,
				ProductionLineId = x.ProductionLineId
			}).Where(x => string.IsNullOrEmpty(userId) || x.CreatedById == userId)
			.Where(x => productionLineId != null || x.ProductionLineId == productionLineId)
			.ToListAsync();
			return result;
		}
		private async Task<List<Hearing>> GetListHearing(string? userId, int? productionLineId)
		{
			var result = new List<Hearing>();
			result = await _context.Hearings.Select(x => new Hearing
			{
				CH = x.CH,
				NUM = x.NUM,
				Model = x.Model,
				Speaker1SPL_1kHz = x.Speaker1SPL_1kHz,
				Result = x.Result,
				DateTime = x.DateTime,
				CreatedById = x.CreatedById,
				ProductionLineId = x.ProductionLineId
			}).Where(x => string.IsNullOrEmpty(userId) || x.CreatedById == userId)
			.Where(x => productionLineId != null || x.ProductionLineId == productionLineId)
			.ToListAsync();
			return result;
		}
	}
}
