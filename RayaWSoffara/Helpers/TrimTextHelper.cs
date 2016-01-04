using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace RayaWSoffara.Helpers
{
    public static class TrimTextHelper
    {
        public static string TrimText(string text, int charCount)
        {
            string trimmed_text = "";
            if (text != null)
            {
                trimmed_text = Regex.Replace(text, "<.*?>", String.Empty); ;

                if (charCount < trimmed_text.Length)
                {
                    trimmed_text = trimmed_text.Substring(0, charCount) + " ...";
                }
            }
            return trimmed_text;
        }
    }
}