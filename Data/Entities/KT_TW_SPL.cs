using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class KT_TW_SPL : BaseNFC
    {
        [MaxLength(20)]
        public string? Grade { get; set; }
        [MaxLength(20)]
        public string? SPL_1kHz { get; set; }
        [MaxLength(20)]
        public string? Polarity { get; set; }
        [MaxLength(20)]
        public string? THD_1kHz { get; set; }
        [MaxLength(20)]
        public string? Impedance_1kHz { get; set; }
    }
}
