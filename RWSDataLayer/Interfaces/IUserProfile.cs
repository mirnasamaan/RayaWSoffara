using System.Web.Security;
using RWSDataLayer.Context;
using System.Collections.Generic;

namespace RWSDataLayer.Interfaces
{
    public interface IUserProfile
    {
        int UserId { get; set; }
        string UserName { get; set; }
        string FirstName { get; set; }
        string DisplayName { get; set; }
        string LastName { get; set; }
        string profileImgUrl { get; set; }
        int articlesCount { get; set; }
        int viewsCount { get; set; }
        int FavTeamId { get; set; }
        string FavTeamName { get; set; }
        int FavCompId { get; set; }
        string FavCompName { get; set; }
        string Twitter { get; set; }
        string Google { get; set; }
        string AboutYou { get; set; }
        List<Post> recentArticles { get; set; }
    }
}
