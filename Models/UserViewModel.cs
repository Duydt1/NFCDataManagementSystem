using Microsoft.AspNetCore.Identity;
using NFC.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace NFC.Models
{
	public class UserViewModel
	{
		public string? Id { get; set; }
		public string? UserName { get; set; }
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z]).*$", ErrorMessage = "Password must contain at least one uppercase character")]
        public string? Password { get; set; }
		public string? Email { get; set; }
		public string? Code { get; set; }
		[DisplayName("First Name")]
		[MaxLength(20)]
		public string? FirstName { get; set; }
		[MaxLength(20)]
		[DisplayName("Middle Name")]
		public string? MiddleName { get; set; }
		[MaxLength(20)]
		[DisplayName("Last Name")]
		public string? LastName { get; set; }
		public string? PhoneNumber { get; set; }
		public bool? Gender { get; set; }
		[DisplayName("Date of Birth")]
		public DateTime? DateOfBirth { get; set; }
		public string? Address { get; set; }
		[DisplayName("Production Line")]
		[Required]
		public int ProductionLineId { get; set; }
		public ProductionLine? ProductionLine { get; set; }
		[DisplayName("User Role")]
		[Required]
		public string RoleId { get; set; }
        public IdentityRole? Role { get; set; }
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z]).*$", ErrorMessage = "Password must contain at least one uppercase character")]
        [DisplayName("Current Password")]
        public string? CurrentPassword { get; set; }
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[A-Z]).*$", ErrorMessage = "Password must contain at least one uppercase character")]
        [DisplayName("New Password")]
        public string? NewPassword { get; set; }
    }
}
