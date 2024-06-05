using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace NFC.Data.Entities
{
	public class NFCUser : IdentityUser
	{
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
		[DisplayName("Full Name")]
		public string? FullName => $"{FirstName} {MiddleName} {LastName}";
		public bool? Gender { get; set; }
		[DisplayName("Date of Birth")]
		public DateTime? DateOfBirth { get; set; }
		[MaxLength(500)]
		public string? Address { get; set; }
		[DisplayName("Production Line")]
		public int ProductionLineId { get; set; }
		public ProductionLine? ProductionLine { get; set; }
		[DisplayName("User Role")]
		public string RoleId { get; set; }
		public IdentityRole? Role { get; set; }
	}
}
