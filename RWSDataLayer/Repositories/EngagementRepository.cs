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
    public class EngagementRepository : BaseRepository<Engagement>
    {
        #region Engagements
        /// <summary>
        /// Add share count
        /// </summary>
        /// <param name="PostId">Post Id</param>
        /// <param name="UserId">User Id</param>
        /// <returns></returns>
        public void AddShareCount(int PostId, int UserId)
        {
            ArticleRepository _postRepo = new ArticleRepository();
            Post post = Context.Posts.Where(i => i.PostId == PostId).FirstOrDefault();
            post.SharesCount++;

            Engagement eng = new Engagement();
            eng.EngTypeId = 1;
            eng.EngTimestamp = DateTime.UtcNow.ToLocalTime();
            eng.PostId = PostId;
            eng.EngUserId = UserId;  // Id of the user who made the engagement
            Context.Engagements.Add(eng);

            Point point = new Point();
            point.UserId = _postRepo.GetPostById(PostId).CreatedBy; //UserId;  // Id of the user who will take the points
            point.isActive = _postRepo.GetPostById(PostId).isOriginal;
            point.PointTimestamp = DateTime.UtcNow.ToLocalTime();
            point.PointTypeId = 1;
            point.PostId = PostId;
            Context.Points.Add(point);

            Context.SaveChanges();
        }

        /// <summary>
        /// Add like count
        /// </summary>
        /// <param name="PostId">Post Id</param>
        /// <param name="UserName">Username</param>
        /// <returns></returns>
        public void AddLikeCount(int PostId, int UserId)
        {
            ArticleRepository _postRepo = new ArticleRepository();
            Post post = Context.Posts.Where(i => i.PostId == PostId).FirstOrDefault();
            post.LikesCount++;

            Engagement eng = new Engagement();
            eng.EngTypeId = 2;
            eng.EngTimestamp = DateTime.UtcNow.ToLocalTime();
            eng.PostId = PostId;
            eng.EngUserId = UserId; // Id of the user who made the engagement
            Context.Engagements.Add(eng);

            Point point = new Point();
            point.UserId = _postRepo.GetPostById(PostId).CreatedBy; //UserId;  // Id of the user who will take the points
            point.isActive = _postRepo.GetPostById(PostId).isOriginal;
            point.PointTimestamp = DateTime.UtcNow.ToLocalTime();
            point.PointTypeId = 2;
            point.PostId = PostId;
            Context.Points.Add(point);

            Context.SaveChanges();
        }

        /// <summary>
        /// Add view count
        /// </summary>
        /// <param name="PostId">Post Id</param>
        /// <param name="UserName">Username</param>
        /// <returns></returns>
        public void AddViewsCount(int PostId, string UserName)
        {
            ArticleRepository _postRepo = new ArticleRepository();
            int? userId = null;
            if (UserName != "")
            {
                userId = Context.RWSUsers.FirstOrDefault(i => i.UserName == UserName).UserId;
            }
            Engagement eng = new Engagement();
            eng.EngTypeId = 3;
            eng.EngTimestamp = DateTime.UtcNow.ToLocalTime();
            eng.PostId = PostId;
            eng.EngUserId = userId; // Id of the user who made the engagement
            Context.Engagements.Add(eng);

            Point point = new Point();
            point.UserId = _postRepo.GetPostById(PostId).CreatedBy; //userId;  // Id of the user who will take the points
            point.isActive = _postRepo.GetPostById(PostId).isOriginal;
            point.PointTimestamp = DateTime.UtcNow.ToLocalTime();
            point.PointTypeId = 3;
            point.PostId = PostId;
            Context.Points.Add(point);

            Context.SaveChanges();
        }

        public IQueryable<EngagementType> GetEngagementTypes(int? startIndex, int count = 10)
        {
            if (startIndex == null)
                return Context.EngagementTypes;
            else if (startIndex > Context.EngagementTypes.Count())
                return null;
            else
                return Context.EngagementTypes.OrderByDescending(i => i.EngType).Skip(startIndex.Value).Take(count);
        }

        public EngagementType GetEngagementTypeById(int EngTypeId)
        {
            return Context.EngagementTypes.FirstOrDefault(i => i.EngTypeId == EngTypeId);
        }

        public EngagementType GetEngagementTypeByName(string EngTypeName)
        {
            if (Context.EngagementTypes.Any(i => i.EngType == EngTypeName))
            {
                return Context.EngagementTypes.FirstOrDefault(i => i.EngType == EngTypeName);
            }
            else
            {
                return null;
            }
        }

        public EngagementType AddEngagementType(EngagementType EngType)
        {
            Context.EngagementTypes.Add(EngType);
            Context.SaveChanges();
            return EngType;
        }

        public EngagementType UpdateEngagementType(EngagementType EngType)
        {
            Context.Entry(EngType).State = System.Data.EntityState.Modified;
            Context.SaveChanges();
            return EngType;
        }

        public EngagementType UpdateEngagementType(int EngTypeId, string EngName, double EngWeight, int EngNewId)
        {
            EngagementType EngType = Context.EngagementTypes.FirstOrDefault(i => i.EngTypeId == EngTypeId);
            DeleteEngagementType(EngType);
            EngType = new EngagementType();
            EngType.EngTypeId = EngNewId;
            EngType.EngType = EngName;
            EngType.EngWeight = EngWeight;
            Context.EngagementTypes.Add(EngType);
            Context.SaveChanges();
            return EngType;
        }

        public void DeleteEngagementType(EngagementType EngType)
        {
            Context.EngagementTypes.Remove(EngType);
            Context.SaveChanges();
        }

        /// <summary>
        /// Get engagement type weight
        /// <param name="EngagementTypeId">Engagement Type Id</param>
        /// <returns></returns>
        /// </summary>
        public double GetEngagementTypeWeight(int EngagementTypeId)
        {
            return Context.EngagementTypes.Where(i => i.EngTypeId == EngagementTypeId).FirstOrDefault().EngWeight.Value;
        }

        /// <summary>
        /// Check if user already liked the post before
        /// </summary>
        /// <param name="PostId">Post Id</param>
        /// <param name="UserId">User Id</param>
        /// <returns></returns>
        public bool isPostLikedByUser(int PostId, int UserId){
            int likes_count = Context.Engagements.Where(i => i.PostId == PostId && i.EngUserId == UserId && i.EngTypeId == 2).Count();
            if (likes_count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets views count of a given user by user Id
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="typeId">Engagement Type id</param>
        /// <returns></returns>
        public int GetEngCountByUserId(int userId, int typeId)
        {
            try
            {
                IQueryable<int> postids = Context.Posts.Where(i => i.CreatedBy == userId && i.IsActive == true).Select(i => i.PostId);
                return Context.Engagements.Where(i => postids.Contains(i.PostId.Value) && i.EngTypeId == typeId).Count();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets views count of a given post by post Id
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <param name="typeId">Engagement Type id</param>
        /// <returns></returns>
        public int GetEngCountByPostId(int postId, int typeId)
        {
            try
            {
                return Context.Engagements.Where(i => i.PostId.Value == postId && i.EngTypeId == typeId).Count();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets views count of a given post by post Id
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <param name="typeId">Engagement Type id</param>
        /// <returns></returns>
        public double GetEngValueByPostId(int postId, int typeId)
        {
            try
            {
                return Context.PointsViews.Where(i => i.PostId.Value == postId && i.PointTypeId.Value == typeId && i.isActive == true).Sum(i => i.PointTypeWeight.Value);
            }
            catch (Exception ex)
            {
                return 0;
            }
        }
        #endregion

        #region Points
        /// <summary>
        /// Add invitation points to user
        /// </summary>
        /// <param name="userEmail">Friend email</param>
        /// <returns></returns>
        public bool AddInvitationPoints(string userEmail)
        {
            try
            {
                UserRepository _userRepo = new UserRepository();
                Point point = new Point();
                point.UserId = _userRepo.GetUserByEmail(userEmail).UserId;
                point.isActive = true;
                point.PointTimestamp = DateTime.UtcNow.ToLocalTime();
                point.PointTypeId = 4;
                Context.Points.Add(point);
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Get total points of a post
        /// </summary>
        /// <param name="postId">Post id</param>
        /// <returns></returns>
        public IQueryable<PointsView> GetPostPoints(int postId)
        {
            try
            {
                return Context.PointsViews.Where(i => i.PostId == postId && i.isActive != null && i.isActive.Value);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get all Point Types.
        /// </summary>
        public IQueryable<PointType> GetPointTypes(int? page, int? count = 10)
        {
            if (page != null && count != null)
            {
                int startIndex = page.Value * count.Value;
                return Context.PointTypes.OrderBy(i => i.PointTypeId).Skip(startIndex).Take(count.Value);
            }
            else
            {
                return Context.PointTypes.OrderBy(i => i.PointTypeId);
            }
        }

        /// <summary>
        /// Get point type by id
        /// </summary>
        /// <param name="pointTypeId">Point type id</param>
        /// <returns></returns>
        public PointType GetPointTypeById(int pointTypeId)
        {
            return Context.PointTypes.FirstOrDefault(i => i.PointTypeId == pointTypeId);
        }

        public bool DeletePointType(int PointTypeId)
        {
            try
            {
                PointType pointType = Context.PointTypes.FirstOrDefault(i => i.PointTypeId == PointTypeId);
                Context.PointTypes.Remove(pointType);

                EngagementType engType = GetEngagementTypeByName(pointType.PointTypeName);
                if (engType != null)
                {
                    Context.EngagementTypes.Remove(engType);
                }
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Update point type
        /// </summary>
        /// <param name="PointTypeId"></param>
        /// <param name="PointWeight"></param>
        /// <returns></returns>
        public PointType UpdatePointType(int PointTypeId, double PointTypeWeight)
        {
            PointType pointType = Context.PointTypes.FirstOrDefault(i => i.PointTypeId == PointTypeId);
            pointType.PointTypeWeight = PointTypeWeight;

            EngagementType engType = GetEngagementTypeByName(pointType.PointTypeName);
            if (engType != null)
            {
                engType.EngWeight = PointTypeWeight;
            }
            Context.SaveChanges();
            return pointType;
        }
        #endregion

    }
}
