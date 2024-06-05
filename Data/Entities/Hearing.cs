using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class Hearing : BaseNFC
    {
        [MaxLength(20)]
        public string? Speaker1SPL_1kHz { get; set; }
    }
}
