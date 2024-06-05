using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class ProductionLine : UserActivity
    {
        public int Id { get; set; }
        [MaxLength(20)]
        public string? Name { get; set; }
        [MaxLength(50)]
        public string? Description { get; set; }
    }
}
