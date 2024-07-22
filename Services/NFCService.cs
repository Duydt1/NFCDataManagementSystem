using Data.Repositories;
using Microsoft.EntityFrameworkCore;
using NFC.Data;
using NFC.Data.Entities;
using NFC.Data.Models;
using NFC.Models;
using System.Drawing.Printing;
using System.Net.WebSockets;
using static NFC.Data.Common.NFCUtil;

namespace NFC.Services
{
    public interface INFCService
	{
		Task<PaginatedList<NFCModel>> GetNFCDashboard(FilterModel filterModel);
	}
	public class NFCService(IServiceProvider serviceProvider) : INFCService
	{
		private readonly IServiceProvider _serviceProvider = serviceProvider;

		public async Task<PaginatedList<NFCModel>> GetNFCDashboard(FilterModel filterModel)
		{
			var nfcModels = new List<NFCModel>();
			var repoTW = _serviceProvider.GetService<IKT_TW_SPLRepository>();
			var repoMIC = _serviceProvider.GetService<IKT_MIC_WF_SPLRepository>();
			var repoHearing = _serviceProvider.GetService<IHearingRepository>();
			var repoSensor = _serviceProvider.GetService<ISensorRepository>();
			var lstTW = await repoTW.GetAllAsync(filterModel);
			var lstMIC = await repoMIC.GetAllAsync(filterModel);
			var lstSensor = await repoSensor.GetAllAsync(filterModel);
			var lstHearing = await repoHearing.GetAllAsync(filterModel);
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
			return new PaginatedList<NFCModel>(nfcModels, nfcModels.Count, filterModel.PageNumber, filterModel.PageSize);
		}
		
	}
}
