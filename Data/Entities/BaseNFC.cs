using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class BaseNFC : UserActivity
    {
        public long Id { get; set; }
        [MaxLength(20)]
        [Required]
        public string? NUM { get; set; }
        [MaxLength(20)]
		[Required]
		public string? Model { get; set; }
        [MaxLength(10)]
		[Required]
		public string? CH { get; set; }
        [MaxLength(10)]
		[Required]
		public string? Result { get; set; }
		[DisplayName("Date Time")]
		public DateTime DateTime { get; set; }
        [Required]
		[DisplayName("Production Line")]
		public int ProductionLineId { get; set; }
        public ProductionLine? ProductionLine { get; set; }
    }
}
