using NFC.Common;
using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class HistoryUpload : UserActivity
    {
        public long Id { get; set; }
        public int Status { get; set; }
        public int Type { get; set; }
        public string Title => $"Upload {NFCCommon.GetNFCType(Type)} data";
        public string strStatus => NFCCommon.GetHistoryStatus(Status);
        public string? FileContent { get; set; }
        [MaxLength(255)]
        public string? Message { get; set; }
		public int? ProductionLineId { get; set; }
		public ProductionLine? ProductionLine { get; set; }
	}
}
