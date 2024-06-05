using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class BaseNFC : UserActivity
    {
        public long Id { get; set; }
        [MaxLength(20)]
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
    }
}
