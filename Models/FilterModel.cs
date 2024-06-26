namespace NFC.Models
{
	public class FilterModel
	{
		public string? SortOrder { get; set; }
		public string? SearchString { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }	
		public int? ProductionLineId { get; set; }	
	}
}
