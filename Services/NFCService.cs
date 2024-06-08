using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NFC.Services
{
	public interface INFCService
	{
		Task<List<NFCModel>> GetNFCDashboard(List<int> productionLineIds);
	}
	public class NFCService(NFCDbContext context) : INFCService
	{
		private readonly NFCDbContext _context = context;

		public async Task<List<NFCModel>> GetNFCDashboard(List<int> productionLineIds)
		{
			var nfcModels = new List<NFCModel>();
			var lstTW = await GetListKTTW(productionLineIds);
			var lstMIC = await GetListKTMIC(productionLineIds);
			var lstSensor = await GetListSensor(productionLineIds);
			var lstHearing = await GetListHearing(productionLineIds);
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
			return [.. nfcModels.OrderByDescending(x => x.DateTime)];
		}

		private async Task<List<KT_TW_SPL>> GetListKTTW(List<int> productionLineIds)
		{
			var query = _context.KT_TW_SPLs.Select(x => new KT_TW_SPL
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
			})
			.AsQueryable();

			if(productionLineIds.Count > 0)
				query.Where(x => productionLineIds.Contains(x.ProductionLineId));

			return await query.ToListAsync();
		}
		private async Task<List<KT_MIC_WF_SPL>> GetListKTMIC(List<int> productionLineIds)
		{
			var query = _context.KT_MIC_WF_SPLs.Select(x => new KT_MIC_WF_SPL
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
			}).AsQueryable();

			if (productionLineIds.Count > 0)
				query.Where(x => productionLineIds.Contains(x.ProductionLineId));

			return await query.ToListAsync();
		}
		private async Task<List<Sensor>> GetListSensor(List<int> productionLineIds)
		{
			var query = _context.Sensors.Select(x => new Sensor
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
			}).AsQueryable();

			if (productionLineIds.Count > 0)
				query.Where(x => productionLineIds.Contains(x.ProductionLineId));

			return await query.ToListAsync();
		}
		private async Task<List<Hearing>> GetListHearing(List<int> productionLineIds)
		{
			var query = _context.Hearings.Select(x => new Hearing
			{
				CH = x.CH,
				NUM = x.NUM,
				Model = x.Model,
				Speaker1SPL_1kHz = x.Speaker1SPL_1kHz,
				Result = x.Result,
				DateTime = x.DateTime,
				CreatedById = x.CreatedById,
				ProductionLineId = x.ProductionLineId
			}).AsQueryable();

			if (productionLineIds.Count > 0)
				query.Where(x => productionLineIds.Contains(x.ProductionLineId));

			return await query.ToListAsync();
		}
	}
}
