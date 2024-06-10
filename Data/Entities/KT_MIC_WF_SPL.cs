using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class KT_MIC_WF_SPL : BaseNFC
    {
        [MaxLength(20)]
		[DisplayName("Speaker1 SPL[100Hz]")]
		public string? SPL_100Hz { get; set; }
        [MaxLength(20)]
		[DisplayName("Speaker1 SPL[1kHz]")]
		public string? SPL_1kHz { get; set; }
        [MaxLength(20)]
		[DisplayName("Speaker1 Polarity")]
		public string? Polarity { get; set; }
        [MaxLength(20)]
		[DisplayName("Speaker1 Impedance[1kHz]")]
		public string? Impedance_1kHz { get; set; }
        [MaxLength(20)]
		[DisplayName("MIC1 SENS at 1kHz")]
		public string? MIC1SENS_1kHz { get; set; }
        [MaxLength(20)]
		[DisplayName("MIC1 Current")]
		public string? MIC1Current { get; set; }
    }
}
