using Data.Models;
using NFC.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace NFC.Models
{
	public class NFCModel
	{
		public int ProductionLineId { get; set; }
		public string CH { get; set; }
		public ProductionLine? ProductionLine { get; set; }
		public ResultModel? SensorTest { get; set; }
		public ResultModel? WFMICTest { get; set; }
		public ResultModel? TWTest { get; set; }
		public ResultModel? HearingTest { get; set; }
	}

	
}
