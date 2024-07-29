using Data.Repositories;
using NFC.Data.Entities;
using NFC.Data.Models;
using NFC.Models;
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
			// Run tasks in parallel
			var lstTW = await GetKT_TW_SPLListAsync(filterModel);
			filterModel.ExistedNum = lstTW.Items.Select(x => x.NUM).Distinct().ToList();
			var lstMIC = await GetKT_MIC_WF_SPLListAsync(filterModel);
			var lstHearing = await GetHearingListAsync(filterModel);
			var lstSensor = await GetSensorListAsync(filterModel);

			// Get the count of items in lstTW
			var lstTWCount = lstTW.Count;

			// Group by NUM and select the last item for each group
			var lstTWItems = lstTW.GroupBy(x => x.NUM).Select(g => g.Last()).ToList();
			var lstMICItems = lstMIC.GroupBy(x => x.NUM).Select(g => g.Last()).ToList();
			var lstHearingItems = lstHearing.GroupBy(x => x.NUM).Select(g => g.Last()).ToList();
			var lstSensorItems = lstSensor.GroupBy(x => x.NUM).Select(g => g.Last()).ToList();

			// Create dictionaries for lookups
			var hearingDict = lstHearingItems.ToDictionary(x => x.NUM);
			var micDict = lstMICItems.ToDictionary(x => x.NUM);
			var sensorDict = lstSensorItems.ToDictionary(x => x.NUM);
			var twDict = lstTWItems.ToDictionary(x => x.NUM);

			// Combine data into NFCModels
			var nfcModels = lstTWItems.Select(item => new NFCModel
			{
				CH = item.CH,
				NUM = item.NUM,
				Model = item.Model,
				DateTime = item.DateTime,
				Hearing = hearingDict.TryGetValue(item.NUM, out var hearing) ? hearing : null,
				KT_TW_SPL = item,
				KT_MIC_WF_SPL = micDict.TryGetValue(item.NUM, out var mic) ? mic : null,
				Sensor = sensorDict.TryGetValue(item.NUM, out var sensor) ? sensor : null,
			}).ToList();

			return new PaginatedList<NFCModel>(nfcModels, lstTWCount, filterModel.PageNumber, filterModel.PageSize);
		}

		private async Task<PaginatedList<KT_TW_SPL>> GetKT_TW_SPLListAsync(FilterModel filterModel)
		{
			using var scope = _serviceProvider.CreateScope();
			var repoTW = scope.ServiceProvider.GetService<IKT_TW_SPLRepository>();
			var result = await repoTW.GetAllAsync(filterModel);
			return result;
		}

		private async Task<List<KT_MIC_WF_SPL>> GetKT_MIC_WF_SPLListAsync(FilterModel filterModel)
		{
			using var scope = _serviceProvider.CreateScope();
			var repoMIC = scope.ServiceProvider.GetService<IKT_MIC_WF_SPLRepository>();
			var result = await repoMIC.GetAllAsync(filterModel);
			return result.Items;
		}

		private async Task<List<Hearing>> GetHearingListAsync(FilterModel filterModel)
		{
			using var scope = _serviceProvider.CreateScope();
			var repoHearing = scope.ServiceProvider.GetService<IHearingRepository>();
			var result = await repoHearing.GetAllAsync(filterModel);
			return result.Items;
		}

		private async Task<List<Sensor>> GetSensorListAsync(FilterModel filterModel)
		{
			using var scope = _serviceProvider.CreateScope();
			var repoSensor = scope.ServiceProvider.GetService<ISensorRepository>();
			var result = await repoSensor.GetAllAsync(filterModel);
			return result.Items;
		}

	}
	
}
