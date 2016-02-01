using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using RWSDataLayer.Context;
using System.Data.Entity;
using System.Data;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Web.Mvc;

namespace RWSDataLayer.Repositories
{
    public class ArticleRepository : BaseRepository<Post>
    {
        #region Post
        /// <summary>
        /// Gets all post types
        /// </summary>
        /// <returns></returns>
        public IQueryable<PostType> GetPostTypes()
        {
            return Context.PostTypes;
        }

        ///<summary>
        ///Get posts by selected date
        ///</summary>
        public IQueryable<Post> GetPostsBySelectedDate(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate != null && toDate != null)
            {
                return Context.Posts.Where(i => i.CreationDate.Value >= fromDate && i.CreationDate.Value <= toDate);
            }
            else if (fromDate != null && toDate == null)
            {
                return Context.Posts.Where(i => i.CreationDate.Value >= fromDate);
            }
            else if (fromDate == null && toDate != null)
            {
                return Context.Posts.Where(i => i.CreationDate.Value <= toDate);
            }
            else
            {
                return Context.Posts;
            }
        }

        /// <summary>
        /// Gets all posts
        /// </summary>
        /// <returns></returns>
        public IQueryable<Post> GetPosts(int tabId, string search, string status, DateTime? from, DateTime? to, string username, int count = 10)
        {
            int startIndex = tabId * count;
            IQueryable<Post> posts = GetPostsBySelectedDate(from, to);
            if (username != null && username != "")
            {
                posts = posts.Where(i => i.RWSUser.UserName == username);
            }
            if (search != null && search != "")
            {
                posts = posts.Where(i => (i.Title.Contains(search) || i.Content.Contains(search) || i.Tags.Where(j => j.TagName.Contains(search)).Count() > 0));
            }
            if (status == "Active")
            {
                return posts.Where(i => i.IsActive != null && i.IsActive.Value).OrderByDescending(i => i.CreationDate).Skip(startIndex).Take(count);
            }
            else if (status == "Inactive")
            {
                return posts.Where(i => (i.IsActive == null || i.IsActive == false)).OrderByDescending(i => i.CreationDate).Skip(startIndex).Take(count);
            }
            else
            {
                return posts.OrderByDescending(i => i.CreationDate).Skip(startIndex).Take(count);
            }
        }

        public IQueryable<Post> GetPosts(int postTypeId, int? startIndex, int count = 10)
        {
            if (startIndex == null)
            {
                return Context.Posts.Where(i => i.PostTypeId == postTypeId).OrderByDescending(i => i.CreationDate);
            }
            else
            {
                return Context.Posts.Where(i => i.PostTypeId == postTypeId).OrderByDescending(i => i.CreationDate).Skip(startIndex.Value).Take(10);
            }
        }

        public IQueryable<Tag> GetTagsForPost(int PostId)
        {
            //return Context.Posts.Where(i => i.PostId == PostId).Select(i => (Tag)i.Tags).ToList();
            return Context.Tags.Where(i => i.Posts.Select(j => j.PostId).Contains(PostId));
        }

        /// <summary>
        /// Get user posts with engagements this month
        /// <param name="UserId">User Id</param>
        /// <param name="MonthId">Month Id</param>
        /// <param name="YearId">Year Id</param>
        /// <returns></returns>
        /// </summary>
        public IQueryable<Post> GetUserPostsWithMonthId(int UserId, int MonthId, int YearId, int Page, int count = 5)
        {
            IQueryable<Post> posts = Context.Posts.Where(i => i.CreatedBy == UserId) //check user id
                .Where(i => i.Points.Any(j => j.PointTimestamp.Value.Month == MonthId && j.PointTimestamp.Value.Year == YearId && i.isOriginal != null && i.isOriginal.Value)) //post has any engagement made this month
                .OrderByDescending(i => i.Points.Where(j => j.PointTimestamp.Value.Month == MonthId && j.PointTimestamp.Value.Year == YearId).Sum(j => j.PointType.PointTypeWeight)) //order by sum of points on this post made this month
                .Skip(Page * count).Take(count);
            return posts;
        }

        /// <summary>
        /// Get user posts count with engagements this month
        /// <param name="UserId">User Id</param>
        /// <param name="MonthId">Month Id</param>
        /// <param name="YearId">Year Id</param>
        /// <returns></returns>
        /// </summary>
        public int GetUserPostsCountWithMonthId(int UserId, int MonthId, int YearId)
        {
            int posts = Context.Posts.Where(i => i.CreatedBy == UserId) //check user id
                .Where(i => i.Points.Any(j => j.PointTimestamp.Value.Month == MonthId && j.PointTimestamp.Value.Year == YearId && i.isOriginal != null && i.isOriginal.Value)) //post has any engagement made this month
                .Count();
            return posts;
        }

        /// <summary>
        /// Gets all active articles
        /// </summary>
        /// <returns></returns>
        public IQueryable<Post> GetActivePosts()
        {
            return Context.Posts.Where(i=>i.IsActive == true).OrderByDescending(i => i.CreationDate);
        }

        /// <summary>
        /// Get posts count
        /// <param name="status">Active or Inactive passed as a string</param>
        /// </summary>
        public int GetPostsCount(string status, DateTime? from, DateTime? to, string username)
        {
            IQueryable<Post> posts = GetPostsBySelectedDate(from, to);
            if (username != null && username != "")
            {
                posts = posts.Where(i => i.RWSUser.UserName == username);
            }
            if (status == "Active")
            {
                return posts.Where(i => i.IsActive != null && i.IsActive.Value).Count();
            }
            else if (status == "Inactive")
            {
                return posts.Where(i => i.IsActive == null || i.IsActive.Value == false).Count();
            }
            else
            {
                return posts.Count();
            }
        }

        /// <summary>
        /// Gets articles with tag (Set count to null if all articles are needed)
        /// </summary>
        /// <returns></returns>
        public IQueryable<Post> GetPostsWithTagIDs(List<int> TagIds, int? count)
        {
            IQueryable<Post> articles;
            if (count != null)
            {
                articles = Context.Posts.Where(i => i.IsActive == true).Where(i => TagIds.Intersect(i.Tags.Select(j => j.TagId)).Count() > 0).OrderByDescending(i => i.CreationDate).Take(count.Value);
            }
            else
            {
                articles = Context.Posts.Where(i => i.IsActive == true).Where(i => TagIds.Intersect(i.Tags.Select(j => j.TagId)).Count() > 0).OrderByDescending(i => i.CreationDate);
            }
            return articles;
        }
        /// <summary>
        /// Gets articles with tag (Set count to null if all articles are needed)
        /// </summary>
        /// <returns></returns>
        public IQueryable<Post> GetPostsWithTagIDs(List<int> TagIds, int? count, IQueryable<Post> postFilteredArticles)
        {
            IQueryable<Post> articles;
            if (count != null)
            {
                articles = postFilteredArticles.Where(i => i.IsActive == true).Where(i => TagIds.Intersect(i.Tags.Select(j => j.TagId)).Count() > 0).OrderByDescending(i => i.CreationDate).Take(count.Value);
            }
            else
            {
                articles = postFilteredArticles.Where(i => i.IsActive == true).Where(i => TagIds.Intersect(i.Tags.Select(j => j.TagId)).Count() > 0).OrderByDescending(i => i.CreationDate);
            }
            return articles;
        }

        /// <summary>
        /// Gets articles with post (Set count to null if all articles are needed)
        /// </summary>
        /// <returns></returns>
        public IQueryable<Post> GetPostsWithPostTypeIDs(List<int> PostIds, int? count)
        {
            IQueryable<Post> posts;
            if (count != null)
            {
                posts = Context.Posts.Where(i => i.IsActive == true).Where(i => PostIds.Contains(i.PostTypeId.Value)).OrderByDescending(i => i.CreationDate).Take(count.Value);
            }
            else
            {
                posts = Context.Posts.Where(i => i.IsActive == true).Where(i => PostIds.Contains(i.PostTypeId.Value)).OrderByDescending(i => i.CreationDate);
            }
            return posts;
        }

        /// <summary>
        /// Gets all user's articles
        /// </summary>
        /// <returns></returns>
        public IQueryable<Post> GetAllUserPosts(int UserId, int count=5)
        {
            IQueryable<Post> articles = Context.Posts.Where(i => i.IsActive == true).Where(i => i.CreatedBy == UserId).OrderByDescending(i => i.CreationDate).Take(count);
            return articles;
        }

        /// <summary>
        /// Gets all user's articles except the current post Id
        /// </summary>
        /// <returns></returns>
        public IQueryable<Post> GetAllUserPostsButOne(int UserId, int PostId, int count = 5)
        {
            IQueryable<Post> articles = Context.Posts.Where(i => i.IsActive == true && i.CreatedBy == UserId && i.PostId != PostId).OrderByDescending(i => i.CreationDate).Take(count);
            return articles;
        }

        public bool SetOriginal(int PostId)
        {
            try
            {
                Post post = Context.Posts.FirstOrDefault(i => i.PostId == PostId);
                post.isOriginal = true;
                IQueryable<Point> points = Context.Points.Where(i => i.PostId == PostId);
                foreach (var item in points)
                {
                    item.isActive = true;
                }
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UnsetOriginal(int PostId)
        {
            try
            {
                Post post = Context.Posts.FirstOrDefault(i => i.PostId == PostId);
                post.isOriginal = false;
                IQueryable<Point> points = Context.Points.Where(i => i.PostId == PostId);
                foreach (var item in points)
                {
                    item.isActive = false;
                }
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool DeactivatePost(int PostId)
        {
            try
            {
                Post post = Context.Posts.Where(i => i.PostId == PostId).FirstOrDefault();
                post.IsActive = false;
                Context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool ActivatePost(int PostId)
        {
            try
            {
                Post post = Context.Posts.Where(i => i.PostId == PostId).FirstOrDefault();
                post.IsActive = true;
                post.ActivationDate = DateTime.Now;
                Context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// Add new article
        /// </summary>
        /// <param name="article">Article object</param>
        /// <returns></returns>
        public Post AddPost(Post post)
        {
            Context.Posts.Add(post);
            try
            {
                Context.SaveChanges();
                return post;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets article by Id
        /// </summary>
        /// <param name="id">Article id</param>
        /// <returns></returns>
        public Post GetPostById(int id)
        {
            return Context.Posts.FirstOrDefault(i => i.PostId == id);
        }

        /// <summary>
        /// Delete a post
        /// </summary>
        /// <param name="post">Post object</param>
        public void DeletePost(Post post)
        {
            post.IsActive = false;
            post.isDeleted = true;
            Context.SaveChanges();
        }

        /// <summary>
        /// Updates an article
        /// </summary>
        /// <param name="updatedArticle">The article object</param>
        /// <returns></returns>
        public Post UpdateArticle(Post updatedArticle, RWSUser updatedBy)
        {
            updatedArticle.UpdateDate = DateTime.Now;
            updatedArticle.UpdatedBy = updatedBy.UserId;
            Context.Entry(updatedArticle).State = System.Data.EntityState.Modified;
            Context.SaveChanges();
            return updatedArticle;
        }

        public IQueryable<Comment> GetAllComments(int PostId)
        {
            return Context.Comments.Where(i => i.CommentPostId == PostId);
        }

        /// <summary>
        /// Add a comment to a post
        /// </summary>
        /// <param name="PostId">The post id</param>
        /// <returns></returns>
        public Comment AddComment(int PostId, string comment, string Username)
        {
            Comment addedComment = new Comment();

            UserRepository _userRepo = new UserRepository();
            int UserId = _userRepo.GetUserByUsername(Username).UserId;

            addedComment.CommentContent = comment;
            addedComment.CommentUserId = UserId;
            addedComment.CommentPostId = PostId;
            addedComment.CommentCreationDate = DateTime.Now;
            addedComment = Context.Comments.Add(addedComment);

            //Context.SaveChanges();
            try
            {
                Context.SaveChanges();
                return Context.Comments.Where(i => i.CommentId == addedComment.CommentId).Include(i => i.RWSUser).FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get comments of posts
        /// </summary>
        /// <param name="PostId">The post id</param>
        /// <param name="index">Start index</param>
        /// <returns></returns>
        public IQueryable<Comment> GetComments(int? index, int PostId)
        {
            if (index == null)
            {
                return Context.Comments.Where(i => i.CommentPostId == PostId);
            }
            else
            {
                return Context.Comments.Where(i => i.CommentPostId == PostId).OrderByDescending(i => i.CommentCreationDate).Skip(index.Value * 10).Take(10);
            }
        }

        public Comment GetCommentById(int CommentId)
        {
            return Context.Comments.FirstOrDefault(i => i.CommentId == CommentId);
        }

        public bool DeleteComment(Comment comment)
        {
            try
            {
                Context.Comments.Remove(comment);
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Activates an article
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        public bool ActivateArticle(Post article)
        {
            try
            {
                article.IsActive = true;
                article.ActivationDate = DateTime.Now;
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public IQueryable<Post> GetRecentArticlesByUserId(int UserId)
        {

            return Context.Posts.Where(i => i.CreatedBy == UserId).Where(i => i.IsActive == true).OrderByDescending(i => i.CreationDate).Take(5);
        }

        public bool ReportComment(int CommentId, int UserId)
        {
            try
            {
                CommentReport report = new CommentReport();
                report.CommentId = CommentId;
                report.UserId = UserId;
                report.ReportTimestamp = DateTime.Now;
                Context.CommentReports.Add(report);
                Comment comment = Context.Comments.FirstOrDefault(i => i.CommentId == CommentId);
                comment.CommentReportCount = comment.CommentReportCount + 1;
                //Context.Comments.FirstOrDefault(i => i.CommentId == CommentId).CommentReportCount++;
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Tags
        /// <summary>
        /// Get all tags
        /// </summary>
        /// <returns></returns>
        public IQueryable<Tag> GetTags()
        {
            return Context.Tags.OrderBy(i=> i.TagName);
        }

        public IQueryable<Tag> GetTags(string term)
        {
            return Context.Tags.Where(i => i.TagName.Contains(term)).OrderBy(i => i.TagName);
        }

        /// <summary>
        /// Get featured tags
        /// </summary>
        /// <returns></returns>
        public IQueryable<Tag> GetFeaturedTags()
        {
            return Context.Tags.Where(i => i.isFeatured == true).OrderBy(i => i.TagName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Tag GetTagById(int id)
        {
            return Context.Tags.FirstOrDefault(u => u.TagId == id);
        }

        public Tag GetTagByTagTypeAndTagTypeId(int TagType, int TagTypeId)
        {
            return Context.Tags.Where(i => i.TagType == TagType && i.TagTypeId == TagTypeId).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Tag GetTagByName(string name)
        {
            return Context.Tags.FirstOrDefault(u => u.TagName == name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PostType GetPostTypeByName(string name)
        {
            return Context.PostTypes.FirstOrDefault(u => u.PostTypeName == name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<Post> GetPopularPosts()
        {
            IQueryable<Tag> featured_tags = GetFeaturedTags();
            List<int> tag_ids = featured_tags.Select(i => i.TagId).ToList();
            return Context.Posts.Where(i => i.IsActive == true).Where(i => tag_ids.Intersect(i.Tags.Select(j => j.TagId)).Count() > 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<int> GetLeaderboardPostIds(DateTime? startDate, DateTime? endDate, int count = 3)
        {
            List<int> leaderPostIds = Context.PointsViews
                .GroupBy(i => new { i.PostId })
                .OrderByDescending(sum => sum.Sum(i => i.PointTypeWeight.Value))
                .Select(i => i.Key.PostId.Value)
                .Take(count).ToList();

            return leaderPostIds;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PostType GetPostTypeByID(int id)
        {
            return Context.PostTypes.FirstOrDefault(u => u.PostTypeId == id);
        }

        /// <summary>
        /// Gets list of tags by their ids
        /// </summary>
        /// <param name="selectedTags">list of ids</param>
        /// <returns></returns>
        public IQueryable<Tag> getSelectedTags(List<int> selectedTags)
        {
            return Context.Tags.Where(m => selectedTags.Contains(m.TagId));
        }

        /// <summary>
        /// Gets list of tags by their names
        /// </summary>
        /// <param name="selectedTags">list of ids</param>
        /// <returns></returns>
        public IQueryable<Tag> getSelectedTags(List<string> selectedTags)
        {
            return Context.Tags.Where(m => selectedTags.Contains(m.TagName));
        }

        /// <summary>
        /// Updates the tags of an article
        /// </summary>
        /// <param name="articleId">Id of the article</param>
        /// <param name="tags">List of tags</param>
        /// <returns></returns>
        public bool UpdatedArticleTags(int articleId, List<Tag> tags)
        {
            Post article = Context.Posts.Where(i => i.PostId == articleId).FirstOrDefault();
            if (article != null)
            {
                if (article.Tags != null)
                {
                    article.Tags.Clear();
                }
                article.Tags = tags;
                Context.Entry(article).State = System.Data.EntityState.Modified;
                Context.SaveChanges();
                return true;
            }
            return false;
        }

        public void AddTag(Tag tag)
        {
            Context.Tags.Add(tag);
            Context.SaveChanges();
        }

        public void DeleteTag(Tag tag)
        {
            Context.Tags.Remove(tag);
            Context.SaveChanges();
        }

        public void UpdateTag(Tag tag)
        {
            var t = Context.Tags.Find(tag.TagId);
            Context.Entry(t).CurrentValues.SetValues(tag);
            Context.SaveChanges();
        }

        public void RemoveTagAsFeatured(int TagId)
        {
            Tag tag = Context.Tags.FirstOrDefault(i => i.TagId == TagId);
            tag.isFeatured = false;
            Context.SaveChanges();
        }

        public void SetTagAsFeatured(int TagId)
        {
            Tag tag = Context.Tags.FirstOrDefault(i => i.TagId == TagId);
            tag.isFeatured = true;
            Context.SaveChanges();
        }

        public IQueryable<Tag> GetAllTags(int startIndex, int count)
        {
            if (startIndex > Context.Tags.Count())
                return null;
            else
                return Context.Tags.OrderByDescending(i => i.TagName).Skip(startIndex).Take(count);
        }

        public IQueryable<Tag> GetTagsBySearchTerm(int startIndex, int count, string term)
        {
            if (startIndex > Context.Tags.Count())
                return null;
            else
                return Context.Tags.Where(i => i.TagName.Contains(term)).OrderByDescending(i => i.TagName).Skip(startIndex).Take(count);
        }

        public bool DeactivateTag(int TagId)
        {
            try
            {
                Tag tag = Context.Tags.Where(i => i.TagId == TagId).FirstOrDefault();
                tag.isActive = false;
                Context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool ActivateTag(int TagId)
        {
            try
            {
                Tag tag = Context.Tags.Where(i => i.TagId == TagId).FirstOrDefault();
                tag.isActive = true;
                Context.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion

    }
}
