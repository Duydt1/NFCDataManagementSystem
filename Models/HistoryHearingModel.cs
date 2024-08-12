using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace NFC.Models
{
	public class HistoryHearingModel
	{
		public string NUM { get; set; }
		public string? Result { get; set; }
		public DateTime DateTime { get; set; }
		public int ProductionLineId { get; set; }
	}
}
