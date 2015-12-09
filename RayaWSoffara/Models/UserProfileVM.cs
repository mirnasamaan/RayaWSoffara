using RWSDataLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWSDataLayer.Interfaces;

namespace RayaWSoffara.Models
{
    public class UserProfileVM : IUserProfile
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string DisplayName { get; set; }
        public string LastName { get; set; }
        public string profileImgUrl { get; set; }
        public int articlesCount { get; set; }
        public int viewsCount { get; set; }
        public int FavTeamId { get; set; }
        public string FavTeamName { get; set; }
        public int FavCompId { get; set; }
        public string FavCompName { get; set; }
        public string Twitter { get; set; }
        public string Google { get; set; }
        public string AboutYou { get; set; }
        public List<Post> recentArticles { get; set; }
    }
}
