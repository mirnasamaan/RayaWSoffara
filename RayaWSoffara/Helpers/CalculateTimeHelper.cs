using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RayaWSoffara.Helpers
{
    public static class CalculateTimeHelper
    {
        public static Object CalculteTime(DateTime time)
        {
            dynamic calculated_time = new System.Dynamic.ExpandoObject();
            calculated_time.unit = "";
            calculated_time.diff = "";

            if ((DateTime.Now - time).TotalSeconds < 60)
            {
                calculated_time.diff = Math.Round((DateTime.Now - time).TotalSeconds);
                calculated_time.unit = "ثواني";
            } 
            else if ((DateTime.Now - time).TotalMinutes < 60)
            {
                calculated_time.diff = Math.Round((DateTime.Now - time).TotalMinutes);
                calculated_time.unit = "دقائق";
            }
            else if ((DateTime.Now - time).TotalHours < 24)
            {
                calculated_time.diff = Math.Round((DateTime.Now - time).TotalHours);
                calculated_time.unit = "ساعات";
            }
            else if ((DateTime.Now - time).TotalDays < 31)
            {
                calculated_time.diff = Math.Round((DateTime.Now - time).TotalDays);
                calculated_time.unit = "أيام";
            }
            return calculated_time;
        }
    }
}