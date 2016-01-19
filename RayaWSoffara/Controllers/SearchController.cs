using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using RWSDataLayer.Repositories;
using RWSDataLayer.Context;
using RayaWSoffara.Models;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using System.Web.UI.HtmlControls;

namespace RayaWSoffara.Controllers
{
    [AllowAnonymous]
    public class SearchController : Controller
    {
        public IndexVM GetPopularPosts(int displayedPageNumber, int count = 8)
        {
            ArticleRepository _articleRepo = new ArticleRepository();
            IndexVM result = new IndexVM();
            IQueryable<Post> articles;

            result.PostsCount = _articleRepo.GetPopularPosts().Count();

            articles = _articleRepo.GetPopularPosts();
            articles = articles.OrderBy(i => i.CreationDate).Skip(displayedPageNumber * count).Take(count);
            foreach (var item in articles)
            {
                result.Posts.Add(new PostVM { PostID = item.PostId, PostTypeID = item.PostTypeId.Value, PostTypeName = _articleRepo.GetPostTypeByID(item.PostTypeId.Value).PostTypeName, PostAuthorID = item.CreatedBy, PostAuthorName = (item.RWSUser.DisplayName == null) ? item.RWSUser.FirstName + " " + item.RWSUser.LastName : item.RWSUser.DisplayName, PostAuthorProfilePicture = item.RWSUser.ProfileImagePath, PostSharesCount = item.SharesCount, PostViewsCount = item.ViewsCount, PostCommentsCount = _articleRepo.GetAllComments(item.PostId).Count(), PostImage = item.FeaturedImage, PostContent = item.Content, PostVideo = item.FeaturedVideo, PostTitle = item.Title, PostAuthorUsername = item.RWSUser.UserName });
            }
            return result; 
        }

        public List<int> GetLeaderboardAuthorIds(DateTime? startDate, DateTime? endDate)
        {
            UserRepository _userRepo = new UserRepository();
            return _userRepo.GetLeaderboardAuthorIds(startDate, endDate);
        }

        public List<Post> GetLeaderboardPosts(DateTime? startDate, DateTime? endDate)
        {
            ArticleRepository _articleRepo = new ArticleRepository();

            List<Post> leaderPosts = new List<Post>();
            List<int> leaderPostIds = _articleRepo.GetLeaderboardPostIds(startDate, endDate);
            foreach (var item in leaderPostIds)
            {
                leaderPosts.Add(_articleRepo.GetPostById(item));
            }
            return leaderPosts;
        }

        public IndexVM GetFilteredArticles(string PostIDs, string TagIDs, int displayedPageNumber, string Username, int count = 8)
        {
            ArticleRepository _articleRepo = new ArticleRepository();

            IQueryable<Post> articles;
            IndexVM result = new IndexVM();

            if (PostIDs != null && PostIDs != "")
            {
                string[] PostNames = PostIDs.Split(',');
                string[] TagNames = TagIDs.Split(',');

                List<int> postIds = new List<int>();
                List<int> tagids = new List<int>();
                bool all = false;

                foreach (var item in PostNames)
                {
                    var name = HttpUtility.UrlDecode(item).Trim();
                    if (name == "الكل")
                    {
                        all = true;
                        break;
                    }
                    if (name == "توب")
                    {
                        name = "قوائم";
                    }
                    postIds.Add(_articleRepo.GetPostTypeByName(name).PostTypeId);
                }

                if (!all)
                {
                    articles = GetFilteredArticlesByPostTypeID(postIds);
                }
                else
                {
                    articles = _articleRepo.GetActivePosts();
                }

                if (TagIDs != null && TagIDs != "")
                {
                    foreach (var item in TagNames)
                    {
                        tagids.Add(_articleRepo.GetTagByName(HttpUtility.UrlDecode(item)).TagId);
                    }

                    articles = GetFilteredArticlesByTags(tagids, articles);
                }

                if (Username != null)
                {
                    UserRepository _userRepo = new UserRepository();
                    articles = _userRepo.GetUserActivePosts(Username, articles);
                }

                result.PostsCount = articles.Count();

                articles = articles.OrderBy(i => i.CreationDate).Skip(displayedPageNumber*count).Take(count);
                foreach (var item in articles)
                {
                    result.Posts.Add(new PostVM { PostID = item.PostId, PostTypeID = item.PostTypeId.Value, PostTypeName = _articleRepo.GetPostTypeByID(item.PostTypeId.Value).PostTypeName, PostAuthorID = item.CreatedBy, PostAuthorName = (item.RWSUser.DisplayName == null)? item.RWSUser.FirstName + " " + item.RWSUser.LastName : item.RWSUser.DisplayName, PostAuthorProfilePicture = item.RWSUser.ProfileImagePath, PostSharesCount = item.SharesCount, PostViewsCount = item.ViewsCount, PostCommentsCount = _articleRepo.GetAllComments(item.PostId).Count(), PostImage = item.FeaturedImage, PostContent = item.Content, PostVideo = item.FeaturedVideo, PostTitle = item.Title, PostAuthorUsername = item.RWSUser.UserName });
                }
                return result;
            }
            else if (TagIDs != null && TagIDs != "")
            {
                string[] PostNames = PostIDs.Split(',');
                string[] TagNames = TagIDs.Split(',');

                List<int> postIds = new List<int>();
                List<int> tagids = new List<int>();

                foreach (var item in TagNames)
                {
                    string s = HttpUtility.UrlDecode(item);
                    tagids.Add(_articleRepo.GetTagByName(HttpUtility.UrlDecode(item)).TagId);
                }

                articles = GetFilteredArticlesByTags(tagids, null);

                if (Username != null)
                {
                    UserRepository _userRepo = new UserRepository();
                    articles = _userRepo.GetUserActivePosts(Username, articles);
                }

                result.PostsCount = articles.Count();

                articles = articles.OrderBy(i => i.CreationDate).Skip(displayedPageNumber*count).Take(count);
                foreach (var item in articles)
                {
                    result.Posts.Add(new PostVM { PostID = item.PostId, PostTypeID = item.PostTypeId.Value, PostTypeName = _articleRepo.GetPostTypeByID(item.PostTypeId.Value).PostTypeName, PostAuthorID = item.CreatedBy, PostAuthorName = (item.RWSUser.DisplayName == null)? item.RWSUser.FirstName + " " + item.RWSUser.LastName : item.RWSUser.DisplayName , PostAuthorProfilePicture = item.RWSUser.ProfileImagePath, PostSharesCount = item.SharesCount, PostViewsCount = item.ViewsCount, PostCommentsCount = _articleRepo.GetAllComments(item.PostId).Count(), PostImage = item.FeaturedImage, PostContent = item.Content, PostVideo = item.FeaturedVideo, PostTitle = item.Title, PostAuthorUsername = item.RWSUser.UserName });
                }
                return result;
            }

            articles = _articleRepo.GetActivePosts();
            if (Username != null)
            {
                UserRepository _userRepo = new UserRepository();
                articles = _userRepo.GetUserActivePosts(Username, articles);
            }
            result.PostsCount = articles.Count();
            articles = articles.OrderByDescending(i => i.CreationDate).Skip(displayedPageNumber*count).Take(count);
            foreach (var item in articles)
            {
                result.Posts.Add(new PostVM { PostID = item.PostId, PostTypeID = item.PostTypeId.Value, PostTypeName = _articleRepo.GetPostTypeByID(item.PostTypeId.Value).PostTypeName, PostAuthorID = item.CreatedBy, PostAuthorName = (item.RWSUser.DisplayName == null) ? item.RWSUser.FirstName + " " + item.RWSUser.LastName : item.RWSUser.DisplayName, PostAuthorProfilePicture = item.RWSUser.ProfileImagePath, PostSharesCount = item.SharesCount, PostViewsCount = item.ViewsCount, PostCommentsCount = _articleRepo.GetAllComments(item.PostId).Count(), PostImage = item.FeaturedImage, PostContent = item.Content, PostVideo = item.FeaturedVideo, PostTitle = item.Title, PostAuthorUsername = item.RWSUser.UserName });
            }
            return result;    
        }

        public IQueryable<Post> GetFilteredArticlesByPostTypeID(List<int> PostIDs)
        {
            ArticleRepository _articleRepo = new ArticleRepository();
            IQueryable<Post> articles = _articleRepo.GetPostsWithPostTypeIDs(PostIDs, null);

            return articles;
        }

        public IQueryable<Post> GetFilteredArticlesByTags(List<int> TagIDs, IQueryable<Post> postFilteredArticles)
        {
            IQueryable<Post> posts;
            ArticleRepository _articleRepo = new ArticleRepository();
            if (postFilteredArticles != null)
            {
                posts = _articleRepo.GetPostsWithTagIDs(TagIDs, null, postFilteredArticles);
            }
            else
            {
                posts = _articleRepo.GetPostsWithTagIDs(TagIDs, null);
            }

            return posts;
        }

        public ActionResult GetFeaturedTags()
        {
            ArticleRepository _articleRepo = new ArticleRepository();
            IQueryable<Tag> tags = _articleRepo.GetFeaturedTags();
            List<TagVM> result = new List<TagVM>();
            foreach (var item in tags)
            {
                result.Add(new TagVM { TagID = item.TagId, TagName = item.TagName });
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetTags(string Term)
        {
            ArticleRepository _articleRepo = new ArticleRepository();
            IQueryable<Tag> tags = _articleRepo.GetTags(Term);
            List<TagVM> result = new List<TagVM>();
            foreach (var item in tags)
            {
                result.Add(new TagVM { TagID = item.TagId, TagName = item.TagName });
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPostTypes()
        {
            ArticleRepository _articleRepo = new ArticleRepository();
            List<PostTypeVM> result = new List<PostTypeVM>();
            IQueryable<PostType> postTypes = _articleRepo.GetPostTypes();
            foreach (var item in postTypes)
            {
                result.Add(new PostTypeVM { PostTypeID = item.PostTypeId, PostTypeName = item.PostTypeName });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }
    }
}
