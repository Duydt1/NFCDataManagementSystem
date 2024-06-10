using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
    public class UserActivity
    {
		[DisplayName("Created By")]
		public string? CreatedById { get; set; }
        public NFCUser? CreatedBy { get; set; }
		[DisplayName("Created By")]
		public DateTime? CreatedOn { get; set; }
		[DisplayName("Modified By")]
		public string? ModifiedById { get; set; }
        public NFCUser? ModifiedBy { get; set; }
		[DisplayName("Modified On")]
		public DateTime? ModifiedOn { get; set; }
    }
}
