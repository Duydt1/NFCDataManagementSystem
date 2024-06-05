using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class UserActivity
    {
		public string? CreatedById { get; set; }
        public NFCUser? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
		public string? ModifiedById { get; set; }
        public NFCUser? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
