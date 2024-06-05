using System.Globalization;

namespace NFC.Extensions
{
    public static class NFCExtension
    {
        public static DateTime StartOfWeek(this DateTime dt)
        {
            var culture = CultureInfo.CurrentCulture;
            var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            var diff = dt.DayOfWeek - firstDayOfWeek;
            return dt.AddDays(-diff).Date;
        }

        public static DateTime StartOfMonth(this DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, 1);
        }
    }
}
