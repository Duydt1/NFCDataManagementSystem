using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class Sensor : BaseNFC
    {
        [MaxLength(20)]
		[DisplayName("DEVICE NO")]
		public string? DeviceNo { get; set; }
        [MaxLength(20)]
		[DisplayName("BATT. VOLT.")]
		public string? BattVolt { get; set; }
    }
}
