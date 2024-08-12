using Data.Models;
using Data.Repositories;
using NFC.Data.Entities;
using NFC.Data.Models;
using NFC.Models;
using static MassTransit.ValidationResultExtensions;
using static NFC.Data.Common.NFCUtil;

namespace NFC.Services
{
	public interface INFCService
	{
		Task<HomePageViewModel> GetNFCDashboard(FilterModel filterModel);
	}
	public class NFCService(IServiceProvider serviceProvider) : INFCService
	{
		private readonly IServiceProvider _serviceProvider = serviceProvider;

		public async Task<HomePageViewModel> GetNFCDashboard(FilterModel filterModel)
		{
			var lstTW = await GetKT_TW_SPLTotalAsync(filterModel);
			var lstMIC = await GetKT_MIC_WF_SPLTotalAsync(filterModel);
			var lstHearing = await GetHearingTotalAsync(filterModel);
			var lstSensor = await GetSensorTotalAsync(filterModel);

			var lstItem = lstTW.Concat(lstMIC).Concat(lstHearing).Concat(lstSensor)
				.GroupBy(item => item.CH)
				.Select(group => new NFCModel
				{
					ProductionLineId = (int)filterModel.ProductionLineId,
					CH = group.Key,
					SensorTest = group.FirstOrDefault(item => lstSensor.Contains(item)) ,
					WFMICTest = group.FirstOrDefault(item => lstMIC.Contains(item)),
					TWTest = group.FirstOrDefault(item => lstTW.Contains(item)),
					HearingTest = group.FirstOrDefault(item => lstHearing.Contains(item)),
				})
				.ToList();

			int totalSensor = lstItem.Sum(x => x.SensorTest != null ? x.SensorTest.Total : 0);
			int totalTW = lstItem.Sum(x => x.TWTest != null ? x.TWTest.Total : 0);
			int totalWF = lstItem.Sum(x => x.WFMICTest != null ? x.WFMICTest.Total : 0);
			int totalHearing = lstItem.Sum(x => x.HearingTest != null ? x.HearingTest.Total : 0);

			// Calculate total Pass and Fail for each test type
			int sensorPass = lstItem.Sum(x => x.SensorTest != null ? x.SensorTest.TotalPass : 0);
			int sensorFail = totalSensor - sensorPass;

			int twPass = lstItem.Sum(x => x.TWTest != null ? x.TWTest.TotalPass : 0);
			int twFail = totalTW - twPass;

			int wfPass = lstItem.Sum(x => x.WFMICTest != null ? x.WFMICTest.TotalPass : 0);
			int wfFail = totalWF - wfPass;

			int hearingPass = lstItem.Sum(x => x.HearingTest != null ? x.HearingTest.TotalPass : 0);
			int hearingFail = totalHearing - hearingPass;

			// Calculate percentage for each test type
			double sensorPercent = totalSensor > 0 ? (sensorFail / (double)totalSensor) * 100 : 0;
			double twPercent = totalTW > 0 ? (twFail / (double)totalTW) * 100 : 0;
			double wfPercent = totalWF > 0 ? (wfFail / (double)totalWF) * 100 : 0;
			double hearingPercent = totalHearing > 0 ? (hearingFail / (double)totalHearing) * 100 : 0;

			return new HomePageViewModel
			{
				TotalSensor = totalSensor,
				TotalTW = totalTW,
				TotalWF = totalWF,
				TotalHearing = totalHearing,
				SensorPass = sensorPass,
				SensorFail = sensorFail,
				WFPass = wfPass,
				WFFail = wfFail,
				TWPass = twPass,
				TWFail = twFail,
				HearingPass = hearingPass,
				HearingFail = hearingFail,
				SensorPercent = sensorPercent,
				TWPercent = twPercent,
				WFPercent = wfPercent,
				HearingPercent = hearingPercent,
				MainList = lstItem,
				HistoryHearingList = await GetHearingListAsync(filterModel)
			}; ;
		}

		private async Task<List<ResultModel>> GetKT_TW_SPLTotalAsync(FilterModel filterModel)
		{
			using var scope = _serviceProvider.CreateScope();
			var repoTW = scope.ServiceProvider.GetService<IKT_TW_SPLRepository>();
			var result = await repoTW.GetTotal(filterModel);
			return result;
		}

		private async Task<List<ResultModel>> GetKT_MIC_WF_SPLTotalAsync(FilterModel filterModel)
		{
			using var scope = _serviceProvider.CreateScope();
			var repoMIC = scope.ServiceProvider.GetService<IKT_MIC_WF_SPLRepository>();
			var result = await repoMIC.GetTotal(filterModel);
			return result;
		}

		private async Task<List<ResultModel>> GetHearingTotalAsync(FilterModel filterModel)
		{
			using var scope = _serviceProvider.CreateScope();
			var repoHearing = scope.ServiceProvider.GetService<IHearingRepository>();
			var result = await repoHearing.GetTotal(filterModel);
			return result;
		}
		private async Task<List<Hearing>> GetHearingListAsync(FilterModel filterModel)
		{
			using var scope = _serviceProvider.CreateScope();
			var repoHearing = scope.ServiceProvider.GetService<IHearingRepository>();
			filterModel.PageSize = 50;
			filterModel.PageNumber = 1;
			var result = await repoHearing.GetAllAsync(filterModel);
			return result.Items;
		}
		private async Task<List<ResultModel>> GetSensorTotalAsync(FilterModel filterModel)
		{
			using var scope = _serviceProvider.CreateScope();
			var repoSensor = scope.ServiceProvider.GetService<ISensorRepository>();
			var result = await repoSensor.GetTotal(filterModel);
			return result;
		}

	}
	
}
