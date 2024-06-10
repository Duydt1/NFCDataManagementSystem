using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class Hearing : BaseNFC
    {
        [MaxLength(20)]
		[DisplayName("Speaker1 SPL[1kHz]")]
		public string? Speaker1SPL_1kHz { get; set; }
    }
}
