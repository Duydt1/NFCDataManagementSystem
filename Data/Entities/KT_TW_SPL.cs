using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class KT_TW_SPL : BaseNFC
    {
        [MaxLength(20)]
        [DisplayName("Speaker GRADE")]
        public string? Grade { get; set; }
        [MaxLength(20)]
		[DisplayName("Speaker1 SPL[1kHz]")]
		public string? SPL_1kHz { get; set; }
        [MaxLength(20)]
		[DisplayName("Speaker1 Polarity")]
		public string? Polarity { get; set; }
        [MaxLength(20)]
		[DisplayName("Speaker1 THD[1KHz]")]
		public string? THD_1kHz { get; set; }
        [MaxLength(20)]
		[DisplayName("Speaker1 Impedance[1KHz]")]
		public string? Impedance_1kHz { get; set; }
    }
}
