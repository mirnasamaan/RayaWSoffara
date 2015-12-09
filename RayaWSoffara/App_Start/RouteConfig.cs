using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RayaWSoffara
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Admin",
                url: "Admin",
                defaults: new { controller = "Admin", action = "Signin", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/Signin",
                url: "Signin",
                defaults: new { controller = "Admin", action = "Signin", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/Signout",
                url: "Admin/SignOut",
                defaults: new { controller = "Admin", action = "Signout", id = UrlParameter.Optional }
            );

            #region User
            routes.MapRoute(
                name: "Admin/Users",
                url: "Admin/Users",
                defaults: new { controller = "Admin", action = "Users", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/AddUser",
                url: "Admin/AddUser",
                defaults: new { controller = "Admin", action = "AddUser", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeleteUser",
                url: "Admin/DeleteUser",
                defaults: new { controller = "Admin", action = "DeleteUser", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/EditUser",
                url: "Admin/EditUser",
                defaults: new { controller = "Admin", action = "EditUser", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeactivateUser",
                url: "Admin/DeactivateUser",
                defaults: new { controller = "Admin", action = "DeactivateUser", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/ActivateUser",
                url: "Admin/ActivateUser",
                defaults: new { controller = "Admin", action = "ActivateUser", id = UrlParameter.Optional }
            );
            #endregion

            #region Regions
            routes.MapRoute(
                name: "Admin/Regions",
                url: "Admin/Regions",
                defaults: new { controller = "Admin", action = "Regions", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/AddRegion",
                url: "Admin/AddRegion",
                defaults: new { controller = "Admin", action = "AddRegion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/EditRegion",
                url: "Admin/EditRegion",
                defaults: new { controller = "Admin", action = "EditRegion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeleteRegion",
                url: "Admin/DeleteRegion",
                defaults: new { controller = "Admin", action = "DeleteRegion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeactivateRegion",
                url: "Admin/DeactivateRegion",
                defaults: new { controller = "Admin", action = "DeactivateRegion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/ActivateRegion",
                url: "Admin/ActivateRegion",
                defaults: new { controller = "Admin", action = "ActivateRegion", id = UrlParameter.Optional }
            );
#endregion

            #region Competitions
            routes.MapRoute(
                name: "Admin/Competitions",
                url: "Admin/Competitions",
                defaults: new { controller = "Admin", action = "Competitions", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/AddCompetition",
                url: "Admin/AddCompetition",
                defaults: new { controller = "Admin", action = "AddCompetition", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/EditCompetition",
                url: "Admin/EditCompetition",
                defaults: new { controller = "Admin", action = "EditCompetition", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeleteCompetition",
                url: "Admin/DeleteCompetition",
                defaults: new { controller = "Admin", action = "DeleteCompetition", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeactivateCompetition",
                url: "Admin/DeactivateCompetition",
                defaults: new { controller = "Admin", action = "DeactivateCompetition", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/ActivateCompetition",
                url: "Admin/ActivateCompetition",
                defaults: new { controller = "Admin", action = "ActivateCompetition", id = UrlParameter.Optional }
            );
            #endregion

            #region Teams
            routes.MapRoute(
                name: "Admin/Teams",
                url: "Admin/Teams",
                defaults: new { controller = "Admin", action = "Teams", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/AddTeam",
                url: "Admin/AddTeam",
                defaults: new { controller = "Admin", action = "AddTeam", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/EditTeam",
                url: "Admin/EditTeam",
                defaults: new { controller = "Admin", action = "EditTeam", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeleteTeam",
                url: "Admin/DeleteTeam",
                defaults: new { controller = "Admin", action = "DeleteTeam", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeactivateTeam",
                url: "Admin/DeactivateTeam",
                defaults: new { controller = "Admin", action = "DeactivateTeam", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/ActivateTeam",
                url: "Admin/ActivateTeam",
                defaults: new { controller = "Admin", action = "ActivateTeam", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/GetTeams",
                url: "Admin/GetTeams",
                defaults: new { controller = "Admin", action = "GetTeams", id = UrlParameter.Optional }
            );
            #endregion

            #region Players
            routes.MapRoute(
                name: "Admin/Players",
                url: "Admin/Players",
                defaults: new { controller = "Admin", action = "Players", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/AddPlayer",
                url: "Admin/AddPlayer",
                defaults: new { controller = "Admin", action = "AddPlayer", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/EditPlayer",
                url: "Admin/EditPlayer",
                defaults: new { controller = "Admin", action = "EditPlayer", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeletePlayer",
                url: "Admin/DeletePlayer",
                defaults: new { controller = "Admin", action = "DeletePlayer", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeactivatePlayer",
                url: "Admin/DeactivatePlayer",
                defaults: new { controller = "Admin", action = "DeactivatePlayer", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/ActivatePlayer",
                url: "Admin/ActivatePlayer",
                defaults: new { controller = "Admin", action = "ActivatePlayer", id = UrlParameter.Optional }
            );
            #endregion

            #region Tags
            routes.MapRoute(
                name: "Admin/Tags",
                url: "Admin/Tags",
                defaults: new { controller = "Admin", action = "Tags", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/AddTag",
                url: "Admin/AddTag",
                defaults: new { controller = "Admin", action = "AddTag", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/EditTag",
                url: "Admin/EditTag",
                defaults: new { controller = "Admin", action = "EditTag", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeleteTag",
                url: "Admin/DeleteTag",
                defaults: new { controller = "Admin", action = "DeleteTag", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeactivateTag",
                url: "Admin/DeactivateTag",
                defaults: new { controller = "Admin", action = "DeactivateTag", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/ActivateTag",
                url: "Admin/ActivateTag",
                defaults: new { controller = "Admin", action = "ActivateTag", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/SetTagAsFeatured",
                url: "Admin/SetTagAsFeatured",
                defaults: new { controller = "Admin", action = "SetTagAsFeatured", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/RemoveTagAsFeatured",
                url: "Admin/RemoveTagAsFeatured",
                defaults: new { controller = "Admin", action = "RemoveTagAsFeatured", id = UrlParameter.Optional }
            );
            #endregion

            #region Images
            routes.MapRoute(
                name: "Admin/Images",
                url: "Admin/Images",
                defaults: new { controller = "Admin", action = "Images", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/AddImages",
                url: "Admin/AddImages",
                defaults: new { controller = "Admin", action = "AddImages", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/EditImage",
                url: "Admin/EditImage",
                defaults: new { controller = "Admin", action = "EditImage", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeleteImage",
                url: "Admin/DeleteImage",
                defaults: new { controller = "Admin", action = "DeleteImage", id = UrlParameter.Optional }
            );
            #endregion

            #region Articles
            routes.MapRoute(
                name: "Admin/Articles",
                url: "Admin/Articles",
                defaults: new { controller = "Admin", action = "Articles", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/AddArticle",
                url: "Admin/AddArticle",
                defaults: new { controller = "Admin", action = "AddArticle", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/EditArticle",
                url: "Admin/EditArticle",
                defaults: new { controller = "Admin", action = "EditArticle", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeleteArticle",
                url: "Admin/DeleteArticle",
                defaults: new { controller = "Admin", action = "DeleteArticle", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeactivateArticle",
                url: "Admin/DeactivateArticle",
                defaults: new { controller = "Admin", action = "DeactivateArticle", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/ActivateArticle",
                url: "Admin/ActivateArticle",
                defaults: new { controller = "Admin", action = "ActivateArticle", id = UrlParameter.Optional }
            );
            #endregion

            #region Opinions
            routes.MapRoute(
                name: "Admin/Opinions",
                url: "Admin/Opinions",
                defaults: new { controller = "Admin", action = "Opinions", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/AddOpinion",
                url: "Admin/AddOpinion",
                defaults: new { controller = "Admin", action = "AddOpinion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/EditOpinion",
                url: "Admin/EditOpinion",
                defaults: new { controller = "Admin", action = "EditOpinion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeleteOpinion",
                url: "Admin/DeleteOpinion",
                defaults: new { controller = "Admin", action = "DeleteOpinion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/DeactivateOpinion",
                url: "Admin/DeactivateOpinion",
                defaults: new { controller = "Admin", action = "DeactivateOpinion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Admin/ActivateOpinion",
                url: "Admin/ActivateOpinion",
                defaults: new { controller = "Admin", action = "ActivateOpinion", id = UrlParameter.Optional }
            );
            #endregion

            routes.MapRoute(
                name: "Login",
                url: "Login",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Register",
                url: "Register",
                defaults: new { controller = "Account", action = "Register", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ExternalRegister",
                url: "ExternalRegister",
                defaults: new { controller = "Account", action = "Register2", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "UpdateProfileImg",
                url: "UpdateProfileImg",
                defaults: new { controller = "Account", action = "UpdateProfileImg", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Popular",
                url: "Popular",
                defaults: new { controller = "Home", action = "Popular", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "Leaderboard",
               url: "Leaderboard",
               defaults: new { controller = "Home", action = "Leaderboard", id = UrlParameter.Optional }
           );
            
            routes.MapRoute(
                name: "Article",
                url: "Article",
                defaults: new { controller = "Article", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Post",
                url: "Post/{id}",
                defaults: new { controller = "Article", action = "ArticleDisplay", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Article/ArticleDisplay",
                url: "Post/{id}",
                defaults: new { controller = "Article", action = "ArticleDisplay", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "AddComment",
                url: "AddComment",
                defaults: new { controller = "Article", action = "AddComment", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ForgotPassword",
                url: "ForgotPassword",
                defaults: new { controller = "Account", action = "ForgotPassword", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ConfirmationSuccess",
                url: "ConfirmationSuccess",
                defaults: new { controller = "Account", action = "ConfirmationSuccess", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ConfirmationFailure",
                url: "ConfirmationFailure",
                defaults: new { controller = "Account", action = "ConfirmationFailure", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "ConfirmationEmailSent",
                url: "ConfirmationEmailSent",
                defaults: new { controller = "Account", action = "ConfirmationEmailSent", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Write",
                url: "Write",
                defaults: new { controller = "Article", action = "Write", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Opinion",
                url: "Opinion",
                defaults: new { controller = "Article", action = "Opinion", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Media",
                url: "Media",
                defaults: new { controller = "Article", action = "Media", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "WriteTopX",
                url: "WriteTopX",
                defaults: new { controller = "Article", action = "WriteTopX", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Profile",
                url: "Profile",
                defaults: new { controller = "Account", action = "Profile", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Points",
                url: "Points",
                defaults: new { controller = "Account", action = "Points", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Settings",
                url: "Settings",
                defaults: new { controller = "Account", action = "Settings", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}