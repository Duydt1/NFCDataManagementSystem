using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class Sensor : BaseNFC
    {
        [MaxLength(20)]
        public string? DeviceNo { get; set; }
        [MaxLength(20)]
        public string? BattVolt { get; set; }
    }
}
