using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using RWSDataLayer;
using RayaWSoffara.Models;

namespace RayaWSoffara
{
    public static class AuthConfig
    {
        public static void RegisterAuth()
        {
            // To let users of this site log in using their accounts from other sites such as Microsoft, Facebook, and Twitter,
            // you must update this site. For more information visit http://go.microsoft.com/fwlink/?LinkID=252166

            //OAuthWebSecurity.RegisterMicrosoftClient(
            //    clientId: "",
            //    clientSecret: "");

            //OAuthWebSecurity.RegisterTwitterClient(
            //    consumerKey: "",
            //    consumerSecret: "");
            Dictionary<String, Object> facebookPermissions = new Dictionary<string, object>();
            facebookPermissions.Add("scope", "email,location, user_photos");

            //local fb key
            //OAuthWebSecurity.RegisterFacebookClient("1464980217131308", "d6d6618eab9e67f73bc973deb0792bc3", "facebook", facebookPermissions);
           
            //staging fb key
            OAuthWebSecurity.RegisterFacebookClient("1490040901291906", "46315ba0c7b2820b11e1c2a151c19f02", "facebook", facebookPermissions);

            //OAuthWebSecurity.RegisterGoogleClient();
        }
    }
}
