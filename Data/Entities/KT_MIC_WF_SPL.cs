using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class KT_MIC_WF_SPL : BaseNFC
    {
        [MaxLength(20)]
        public string? SPL_100Hz { get; set; }
        [MaxLength(20)]
        public string? SPL_1kHz { get; set; }
        [MaxLength(20)]
        public string? Polarity { get; set; }
        [MaxLength(20)]
        public string? Impedance_1kHz { get; set; }
        [MaxLength(20)]
        public string? MIC1SENS_1kHz { get; set; }
        [MaxLength(20)]
        public string? MIC1Current { get; set; }
    }
}
