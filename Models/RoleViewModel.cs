using System.ComponentModel.DataAnnotations;

namespace NFC.Models
{
	public class RoleViewModel
	{
		public string? RoleId { get; set; }
		[Required]
		public string RoleName { get; set; }
	}
}
