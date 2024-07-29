using NFC.Data.Entities;
using NuGet.Protocol.Plugins;
using System.Net;

namespace NFC.Models
{
	public class UploadNFCDataResponse
	{
		public HttpStatusCode Code { get; set; }
		public string Message { get; set; }
		public string NFCDatas { get; set; }
	}
}
