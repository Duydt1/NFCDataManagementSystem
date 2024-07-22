using NFC.Data.Common;
using NFC.Models;
using System.Buffers.Text;

namespace NFC.Services
{
	public interface IFileUploadService
    {
        
        Task<UploadNFCDataResponse> ValidateFileCsvAsync(int type, string uploadFile);
    }
    public class FileUploadService(IServiceProvider serviceProvider, ILogger<FileUploadService> logger) : IFileUploadService
    {
        private readonly ILogger<FileUploadService> _logger = logger;
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public async Task<UploadNFCDataResponse> ValidateFileCsvAsync(int type, string uploadFile)
        {
            var reponse = new UploadNFCDataResponse();
            var isValidBase64 = Base64.IsValid(uploadFile, out int decodedLength);
            if (!isValidBase64)
            {
                reponse.Code = System.Net.HttpStatusCode.BadRequest;
                reponse.Message = "File không đúng định dạng base64";
            }
            else
            {
                byte[] bytes = Convert.FromBase64String(uploadFile);
                using var ms = new MemoryStream(bytes);
                using var reader = new StreamReader(ms);
                switch (type)
                {
                    case (int)NFCCommon.NFCType.KT_TW_SPL:
                        reponse = await ValidateDataKTTW(reader);
                        break;
                    case (int)NFCCommon.NFCType.KT_MIC_WF_SPL:
                        reponse = await ValidateDataKTMIC(reader);
                        break;
                    case (int)NFCCommon.NFCType.SENSOR:
                        reponse = await ValidateDataSensor(reader);
                        break;
                    case (int)NFCCommon.NFCType.HEARING:
                        reponse = await ValidateDataHearing(reader);
                        break;
                }
            }

            return reponse;
        }

        private static async Task<UploadNFCDataResponse> ValidateDataKTTW(StreamReader reader)
        {
            var lstKTTW = await NFCReadFile.ReadListKTTWAsync(reader);
            var result = new UploadNFCDataResponse();
            if (lstKTTW.Count < 1)
                result.Message = "List data is empty";
            else
            {
                var line = 0;
                foreach (var item in lstKTTW)
                {
                    line += 1;
                    result.Message += string.IsNullOrEmpty(item.NUM) ? $"NUM is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Model) ? $"Model is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.CH) ? $"CH is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Result) ? $"Result is null; " : "";
                    result.Message += item.DateTime != DateTime.MinValue ? "" : $"Date Time is null; ";
                    //result.Message += string.IsNullOrEmpty(item.Grade) ? $"Speaker GRADE is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.SPL_1kHz) ? $"Speaker1 SPL[1kHz] is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Polarity) ? $"Speaker1 Polarity is null; " : "";
                    //result.Message += string.IsNullOrEmpty(item.THD_1kHz) ? $"Speaker1 THD[1KHz] is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Impedance_1kHz) ? $"Speaker1 Impedance[1KHz] is null; " : "";
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        string strNum = $"Error Data in line {line} : ";
                        result.Message = strNum + result.Message;
                    }
                }
            }

            if (!string.IsNullOrEmpty(result.Message))
                result.Code = System.Net.HttpStatusCode.BadRequest;

            return result;
        }
        private static async Task<UploadNFCDataResponse> ValidateDataKTMIC(StreamReader reader)
        {
            var lstKTTW = await NFCReadFile.ReadListKTMICAsync(reader);
            var result = new UploadNFCDataResponse();
            if (lstKTTW.Count < 1)
                result.Message = "List data is empty";
            else
            {
                var line = 0;
                foreach (var item in lstKTTW)
                {
                    line += 1;
                    result.Message += string.IsNullOrEmpty(item.NUM) ? $"NUM is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Model) ? $"Model is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.CH) ? $"CH is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Result) ? $"Result is null; " : "";
                    result.Message += item.DateTime != DateTime.MinValue ? "" : $"Date Time is null; ";
                    //result.Message += string.IsNullOrEmpty(item.SPL_100Hz) ? $"Speaker1 SPL[100Hz] is null;" : "";
                    result.Message += string.IsNullOrEmpty(item.SPL_1kHz) ? $"Speaker1 SPL[1kHz] is null;" : "";
                    result.Message += string.IsNullOrEmpty(item.Polarity) ? $"Speaker1 Polarity is null;" : "";
                    //result.Message += string.IsNullOrEmpty(item.Impedance_1kHz) ? $"Speaker1 Impedance[1kHz] is null;" : "";
                    result.Message += string.IsNullOrEmpty(item.MIC1SENS_1kHz) ? $"MIC1 SENS at 1kHz is null;" : "";
                    result.Message += string.IsNullOrEmpty(item.MIC1Current) ? $"MIC1 Current is null;" : "";
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        string strNum = $"Error Data in line {line} : ";
                        result.Message = strNum + result.Message;
                    }

                }
            }

            if (!string.IsNullOrEmpty(result.Message))
                result.Code = System.Net.HttpStatusCode.BadRequest;

            return result;
        }
        private static async Task<UploadNFCDataResponse> ValidateDataSensor(StreamReader reader)
        {
            var lstKTTW = await NFCReadFile.ReadListSensorAsync(reader);
            var result = new UploadNFCDataResponse();
            if (lstKTTW.Count < 1)
                result.Message = "List data is empty";
            else
            {
                var line = 0;
                foreach (var item in lstKTTW)
                {
                    line += 1;
                    result.Message += string.IsNullOrEmpty(item.NUM) ? $"NUM is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Model) ? $"Model is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.CH) ? $"CH is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Result) ? $"Result is null; " : "";
                    result.Message += item.DateTime != DateTime.MinValue ? "" : $"Date Time is null; ";
                    result.Message += string.IsNullOrEmpty(item.DeviceNo) ? $"DEVICE NO is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.BattVolt) ? $"BATT. VOLT. is null; " : "";
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        string strNum = $"Error Data in line {line} : ";
                        result.Message = strNum + result.Message;
                    }
                }
            }

            if (!string.IsNullOrEmpty(result.Message))
                result.Code = System.Net.HttpStatusCode.BadRequest;

            return result;
        }
        private static async Task<UploadNFCDataResponse> ValidateDataHearing(StreamReader reader)
        {
            var lstKTTW = await NFCReadFile.ReadListHearingAsync(reader);
            var result = new UploadNFCDataResponse();
            if (lstKTTW.Count < 1)
                result.Message = "List data is empty";
            else
            {
                var line = 0;
                foreach (var item in lstKTTW)
                {
                    line += 1;
                    result.Message += string.IsNullOrEmpty(item.NUM) ? $"NUM is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Model) ? $"Model is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.CH) ? $"CH is null; " : "";
                    result.Message += string.IsNullOrEmpty(item.Result) ? $"Result is null; " : "";
                    result.Message += item.DateTime != DateTime.MinValue ? "" : $"Date Time is null; ";
                    result.Message += string.IsNullOrEmpty(item.Speaker1SPL_1kHz) ? $"Speaker1 SPL[1kHz] is null; " : "";
                    if (!string.IsNullOrEmpty(result.Message))
                    {
                        string strNum = $"Error Data in line {line} : ";
                        result.Message = strNum + result.Message;
                    }
                }
            }
            if (!string.IsNullOrEmpty(result.Message))
                result.Code = System.Net.HttpStatusCode.BadRequest;

            return result;
        }

    }
}
