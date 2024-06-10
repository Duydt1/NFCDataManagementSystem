using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using System.ComponentModel.DataAnnotations;

namespace NFC.Models
{
    public class UploadNFCDataRequest
    {
		[Required]
		public int NFCType { get; set; }
		[Required]
		public string NFCContent { get; set; }
    }
}
