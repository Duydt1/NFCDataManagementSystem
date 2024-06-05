namespace NFC.Common
{
    public class NFCCommon
    {
		public enum NFCType
        {
            KT_TW_SPL = 1,
            KT_MIC_WF_SPL = 2,
            SENSOR = 3,
            HEARING = 4,
        }

		public enum HistoryStatus
		{
			New = 1,
			Processing = 2,
			Completed = 3,
			Declined = 4,
			Pending = 5,
		}

		public static string GetNFCType(int nfcTypeCode)
        {
            switch (nfcTypeCode)
            {
                case 1:
                    return "KT TW SPL";
                case 2:
                    return "KT MIC & WF SPL ";
                case 3:
                    return "SENSOR";
                case 4:
                    return "HEARING";
                default:
                    return "";
            }
        }

        public static string GetHistoryStatus(int historyStatus)
        {
            switch (historyStatus)
            {
                case 1:
                    return "New";
                case 2:
                    return "Processing";
                case 3:
                    return "Completed";
                case 4:
                    return "Declined";
                default:
                    return "";
            }
        }
    }
}
