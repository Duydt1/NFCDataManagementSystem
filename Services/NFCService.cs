using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NFC.Services
{
	public interface INFCService
	{
		Task<List<NFCModel>> GetNFCDashboard(FilterModel filterModel);
	}
	public class NFCService(NFCDbContext context) : INFCService
	{
		private readonly NFCDbContext _context = context;

		public async Task<List<NFCModel>> GetNFCDashboard(FilterModel filterModel)
		{
			var nfcModels = new List<NFCModel>();
			var lstTW = await GetListKTTW(filterModel);
			var lstMIC = await GetListKTMIC(filterModel);
			var lstSensor = await GetListSensor(filterModel);
			var lstHearing = await GetListHearing(filterModel);
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

		private async Task<List<KT_TW_SPL>> GetListKTTW(FilterModel filterModel)
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
			if (!string.IsNullOrEmpty(filterModel.SearchString))
			{
				query.Where(x => x.NUM.Contains(filterModel.SearchString) || x.Model.Contains(filterModel.SearchString));
			}

			if (!filterModel.FromDate.HasValue)
				filterModel.FromDate = DateTime.Now.AddMonths(-2);

			query = query.Where(h => h.DateTime >= filterModel.FromDate);

			if (!filterModel.ToDate.HasValue)
				filterModel.ToDate = DateTime.Now;
			query = query.Where(h => h.DateTime <= filterModel.ToDate);

			if (filterModel.ProductionLineId > 0)
				query.Where(x => x.ProductionLineId == filterModel.ProductionLineId);

			return await query.ToListAsync();
		}
		private async Task<List<KT_MIC_WF_SPL>> GetListKTMIC(FilterModel filterModel)
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

			if (!string.IsNullOrEmpty(filterModel.SearchString))
			{
				query.Where(x => x.NUM.Contains(filterModel.SearchString) || x.Model.Contains(filterModel.SearchString));
			}

			if (!filterModel.FromDate.HasValue)
				filterModel.FromDate = DateTime.Now.AddMonths(-2);

			query = query.Where(h => h.DateTime >= filterModel.FromDate);

			if (!filterModel.ToDate.HasValue)
				filterModel.ToDate = DateTime.Now;
			query = query.Where(h => h.DateTime <= filterModel.ToDate);

			if (filterModel.ProductionLineId > 0)
				query.Where(x => x.ProductionLineId == filterModel.ProductionLineId);

			return await query.ToListAsync();
		}
		private async Task<List<Sensor>> GetListSensor(FilterModel filterModel)
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

			if (!string.IsNullOrEmpty(filterModel.SearchString))
			{
				query.Where(x => x.NUM.Contains(filterModel.SearchString) || x.Model.Contains(filterModel.SearchString));
			}

			if (!filterModel.FromDate.HasValue)
				filterModel.FromDate = DateTime.Now.AddMonths(-2);

			query = query.Where(h => h.DateTime >= filterModel.FromDate);

			if (!filterModel.ToDate.HasValue)
				filterModel.ToDate = DateTime.Now;
			query = query.Where(h => h.DateTime <= filterModel.ToDate);

			if (filterModel.ProductionLineId > 0)
				query.Where(x => x.ProductionLineId == filterModel.ProductionLineId);

			return await query.ToListAsync();
		}
		private async Task<List<Hearing>> GetListHearing(FilterModel filterModel)
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

			if (!string.IsNullOrEmpty(filterModel.SearchString))
			{
				query.Where(x => x.NUM.Contains(filterModel.SearchString) || x.Model.Contains(filterModel.SearchString));
			}

			if (!filterModel.FromDate.HasValue)
				filterModel.FromDate = DateTime.Now.AddMonths(-2);

			query = query.Where(h => h.DateTime >= filterModel.FromDate);

			if (!filterModel.ToDate.HasValue)
				filterModel.ToDate = DateTime.Now;
			query = query.Where(h => h.DateTime <= filterModel.ToDate);

			if (filterModel.ProductionLineId > 0)
				query.Where(x => x.ProductionLineId == filterModel.ProductionLineId);

			return await query.ToListAsync();
		}
	}
}
