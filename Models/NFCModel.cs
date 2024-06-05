using NFC.Data.Entities;
using System.ComponentModel.DataAnnotations;

namespace NFC.Models
{
	public class NFCModel
	{
		public string? NUM { get; set; }
		[MaxLength(20)]
		public string? Model { get; set; }
		[MaxLength(10)]
		public string? CH { get; set; }
		[MaxLength(10)]
		public string? Result { get; set; }
		public DateTime DateTime { get; set; }
		[Required]
		public int ProductionLineId { get; set; }
		public ProductionLine? ProductionLine { get; set; }
		public Hearing? Hearing { get; set; }
		public Sensor? Sensor { get; set; }
		public KT_TW_SPL? KT_TW_SPL { get; set; }
		public KT_MIC_WF_SPL? KT_MIC_WF_SPL { get; set; }
	}
}
