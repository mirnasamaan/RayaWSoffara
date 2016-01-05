using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace RayaWSoffara.Helpers
{
    public static class CaptchaHelper
    {
        public static bool ValidateCaptcha(string privatekey, string response, string remoteip)
        {
            var data = new NameValueCollection
            {
                { "secret", privatekey },
                { "response", response },
                { "remoteip", remoteip }
            };

            using (var client = new WebClient())
            {

                var res = client.UploadValues("https://www.google.com/recaptcha/api/siteverify", data: data);

                var result = ASCIIEncoding.ASCII.GetString(res);

                if (result.Contains("false"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }
}