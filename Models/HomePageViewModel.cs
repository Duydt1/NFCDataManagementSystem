using NFC.Data.Entities;
using static NFC.Data.Common.NFCUtil;

namespace NFC.Models
{
	public class HomePageViewModel
	{
		public int TotalSensor { get; set; }
		public int TotalTW { get; set; }
		public int TotalWF { get; set; }
		public int TotalHearing { get; set; }

		public int SensorPass { get; set; }
		public int SensorFail { get; set; }
		public int TWPass { get; set; }
		public int TWFail { get; set; }
		public int WFPass { get; set; }
		public int WFFail { get; set; }
		public int HearingPass { get; set; }
		public int HearingFail { get; set; }

		public double SensorPercent { get; set; }
		public double TWPercent { get; set; }
		public double WFPercent { get; set; }
		public double HearingPercent { get; set; }
		public List<NFCModel> MainList { get; set; }
		public List<Hearing> HistoryHearingList { get; set; }
	}

}
