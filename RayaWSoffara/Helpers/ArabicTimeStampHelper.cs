using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RayaWSoffara.Helpers
{
    public static class ArabicTimeStampHelper
    {
        public static string ArabicTimeStamp(DateTime time)
        {
            int dow = (int)time.DayOfWeek;
            int dom = time.Day;
            int mon = time.Month;
            int yr = time.Year;
            string tod = time.ToString("tt");

            string DayOfWeek, DayOfMonth, Month, Year, Hour, TimeOfDay, result;

            switch (dow)
            {
                case 1: DayOfWeek = "الأحد"; break;
                case 2: DayOfWeek = "الأثنين"; break;
                case 3: DayOfWeek = "الثلاثاء"; break;
                case 4: DayOfWeek = "الأربعاء"; break;
                case 5: DayOfWeek = "الخميس"; break;
                case 6: DayOfWeek = "الجمعة"; break;
                case 7: DayOfWeek = "السبت"; break;
                default: DayOfWeek = ""; break;
            }

            DayOfMonth = dom.ToString();

            switch (mon)
            {
                case 1: Month = "يناير"; break;
                case 2: Month = "فبراير"; break;
                case 3: Month = "مارس"; break;
                case 4: Month = "ابريل"; break;
                case 5: Month = "مايو"; break;
                case 6: Month = "يونيو"; break;
                case 7: Month = "يوليو"; break;
                case 8: Month = "أغسطس"; break;
                case 9: Month = "سبتمبر"; break;
                case 10: Month = "أكتوبر"; break;
                case 11: Month = "نوفمبر"; break;
                case 12: Month = "ديسمبر"; break;
                default: Month = ""; break;
            }

            Year = yr.ToString();
            Hour = time.ToString("hh:mm");
            
            switch (tod)
            {
                case "AM": TimeOfDay = "صباحا"; break;
                case "PM": TimeOfDay = "مساء"; break;
                default: TimeOfDay = ""; break;
            }

            Hour = Hour.PadLeft(7, ' ');
            result = DayOfWeek + " " + DayOfMonth + " " + Month + " " + Year + " " + Hour + " " + TimeOfDay;

            return result;
        }
    }
}