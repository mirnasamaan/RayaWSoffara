using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using RWSDataLayer.Context;
using RWSDataLayer.Interfaces;
using System.Data.Entity;
using System.Security.Cryptography;
using System.Data.Objects.SqlClient;
using System.Data.Objects;

namespace RWSDataLayer.Repositories
{
    public class UserRepository : BaseRepository<RWSUser>
    {
        #region Regular Users
        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        public int GetUsersCount(string status, string search, DateTime? from, DateTime? to)
        {
            IQueryable<RWSUser> users = GetUsersBySelectedDate(from, to);
            if (status == "Active")
            {
                users = users.Where(i => i.IsConfirmed.Value);
            }
            else if (status == "Inactive")
            {
                users = users.Where(i => !i.IsConfirmed.Value);
            }
            if (search != null && search != "")
            {
                users = users.Where(i => i.UserName.Contains(search) || SqlFunctions.StringConvert((double)i.UserId).Contains(search) || i.Email.Contains(search));
            }
            return users.Count();
        }

        /// <summary>
        /// Get leaderboard users
        /// </summary>
        /// <returns></returns>
        public List<int> GetLeaderboardAuthorIds(DateTime? startDate, DateTime? endDate, int count = 5)
        {
            List<int> leaderIds;

            if (startDate == null && endDate == null)
            {
                leaderIds = Context.PointsViews
                    .GroupBy(i => new { i.UserId })
                    .OrderByDescending(sum => sum.Sum(i => i.PointTypeWeight.Value))
                    .Select(i => i.Key.UserId)
                    .Take(count).ToList();
            }
            else
            {
                startDate = startDate.Value.Date;
                endDate = endDate.Value.Date;
                leaderIds = Context.PointsViews
                    .Where(i => EntityFunctions.TruncateTime(i.PointTimestamp.Value) >= startDate && EntityFunctions.TruncateTime(i.PointTimestamp.Value) <= endDate)
                    .GroupBy(i => new { i.UserId })
                    .OrderByDescending(sum => sum.Sum(i => i.PointTypeWeight.Value))
                    .Select(i => i.Key.UserId)
                    .Take(count).ToList();
            }

            return leaderIds;
        }

        ///<summary>
        ///Get users by selected date
        ///</summary>
        public IQueryable<RWSUser> GetUsersBySelectedDate(DateTime? fromDate, DateTime? toDate)
        {
            if (fromDate != null && toDate != null)
            {
                fromDate = fromDate.Value.Date;
                toDate = toDate.Value.Date;
                return Context.RWSUsers.Where(i => EntityFunctions.TruncateTime(i.CreationDate.Value) >= fromDate && EntityFunctions.TruncateTime(i.CreationDate.Value) <= toDate);
            }
            else if (fromDate != null && toDate == null)
            {
                fromDate = fromDate.Value.Date;
                return Context.RWSUsers.Where(i => EntityFunctions.TruncateTime(i.CreationDate.Value) >= fromDate);
            }
            else if (fromDate == null && toDate != null)
            {
                toDate = toDate.Value.Date;
                return Context.RWSUsers.Where(i => EntityFunctions.TruncateTime(i.CreationDate.Value) <= toDate);
            }
            else
            {
                return Context.RWSUsers;
            }
        }


        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        public IQueryable<RWSUser> GetAllUsers(int? page, int? size, string status, DateTime? fromDate, DateTime? toDate, out int totalNo)
        {
            int noOfItems = 0;
            if (page != null && size != null)
            {
                noOfItems = page.Value * size.Value;
            }
            IQueryable<RWSUser> users = GetUsersBySelectedDate(fromDate, toDate);
            if (status == "Active")
            {
                totalNo = Context.RWSUsers.Where(i => i.IsConfirmed.Value).Count();
                users = users.Where(i => i.IsConfirmed.Value).OrderBy(i => i.UserName);
                if (page != null && size != null)
                {
                    return users.Skip(noOfItems).Take(size.Value);
                }
                return users;
            }
            else if (status == "Inactive")
            {
                totalNo = Context.RWSUsers.Where(i => !i.IsConfirmed.Value).Count();
                users = users.Where(i => !i.IsConfirmed.Value).OrderBy(i => i.UserName);
                if (page != null && size != null)
                {
                    return users.Skip(noOfItems).Take(size.Value);
                }
                return users;
            }
            else
            {
                totalNo = Context.RWSUsers.Count();
                users = users.OrderBy(i => i.UserName);
                if (page != null && size != null)
                {
                    return users.Skip(noOfItems).Take(size.Value);
                }
                return users;
            }
        }

        public IQueryable<RWSUser> GetUsersBySearchTerm(int? startIndex, int? count, string term, string status, DateTime? fromDate, DateTime? toDate)
        {
            if (status == "Active")
            {
                if (startIndex != null && startIndex > Context.RWSUsers.Where(i => i.IsConfirmed.Value).Count())
                    return null;
                else
                {
                    IQueryable<RWSUser> users = GetUsersBySelectedDate(fromDate, toDate);
                    users = users.Where(i => i.UserName.Contains(term) || SqlFunctions.StringConvert((double)i.UserId).Contains(term) || i.Email.Contains(term));
                    if (startIndex != null && count != null)
                    {
                        return users.Where(i => i.IsConfirmed.Value).OrderByDescending(i => i.UserName).Skip(startIndex.Value).Take(count.Value);
                    }
                    else
                    {
                        return users.Where(i => i.IsConfirmed.Value).OrderByDescending(i => i.UserName);
                    }
                }
            }
            else if (status == "Inactive")
            {
                if (startIndex != null && startIndex > Context.RWSUsers.Where(i => !i.IsConfirmed.Value).Count())
                    return null;
                else
                {
                    IQueryable<RWSUser> users = GetUsersBySelectedDate(fromDate, toDate);
                    users = users.Where(i => i.UserName.Contains(term) || SqlFunctions.StringConvert((double)i.UserId).Contains(term) || i.Email.Contains(term));
                    if (startIndex != null && count != null)
                    {
                        return users.Where(i => !i.IsConfirmed.Value).OrderByDescending(i => i.UserName).Skip(startIndex.Value).Take(count.Value);
                    } else {
                        return users.Where(i => !i.IsConfirmed.Value).OrderByDescending(i => i.UserName);
                    }
                }
            }
            else
            {
                if (startIndex != null && startIndex > Context.RWSUsers.Count())
                    return null;
                else
                {
                    IQueryable<RWSUser> users = GetUsersBySelectedDate(fromDate, toDate);
                    users = users.Where(i => i.UserName.Contains(term) || SqlFunctions.StringConvert((double)i.UserId).Contains(term) || i.Email.Contains(term));
                    if (startIndex != null && count != null)
                    {
                        return users.OrderByDescending(i => i.UserName).Skip(startIndex.Value).Take(count.Value);
                    }else {
                        return users.OrderByDescending(i => i.UserName).Skip(startIndex.Value);
                    }
                }
            }
        }

        public IQueryable<RWSUser> GetUsersByCreationDate(DateTime day)
        {
            try
            {
                day = day.Date;
                return Context.RWSUsers.Where(i => EntityFunctions.TruncateTime(i.CreationDate.Value) == day);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IQueryable<RWSUser> GetUsersByActivationDate(DateTime day)
        {
            try
            {
                day = day.Date;
                return Context.RWSUsers.Where(i => EntityFunctions.TruncateTime(i.ConfirmationDate.Value) == day);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IQueryable<RWSUser> GetPendingUsersByDate(DateTime day)
        {
            try
            {
                day = day.Date;
                return Context.RWSUsers.Where(i => EntityFunctions.TruncateTime(i.CreationDate.Value) == day && i.IsConfirmed != true);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IQueryable<RWSUser> GetUsersByCreationDate(int month, int year)
        {
            try
            {
                return Context.RWSUsers.Where(i => i.CreationDate.Value.Month == month && i.CreationDate.Value.Year == year);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IQueryable<RWSUser> GetUsersByActivationDate(int month, int year)
        {
            try
            {
                return Context.RWSUsers.Where(i => i.IsConfirmed == true).Where(i => i.ConfirmationDate.Value.Month == month && i.ConfirmationDate.Value.Year == year);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public IQueryable<RWSUser> GetPendingUsersByMonth(int month, int year)
        {
            try
            {
                return Context.RWSUsers.Where(i => i.CreationDate.Value.Month == month && i.CreationDate.Value.Year == year && i.IsConfirmed != true);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get all active users.
        /// </summary>
        /// <param name="from">Start date</param>
        /// <param name="to">End date</param>
        /// <returns></returns>
        public IQueryable<RWSUser> GetAllActiveUsers(DateTime? from, DateTime? to)
        {
            if (from != null && to != null)
            {
                from = from.Value.Date;
                to = to.Value.Date;
                return Context.RWSUsers.Where(i => i.IsConfirmed == true && EntityFunctions.TruncateTime(i.ConfirmationDate.Value) >= from && EntityFunctions.TruncateTime(i.ConfirmationDate.Value) <= to);
            }
            else if (from != null && to == null)
            {
                from = from.Value.Date;
                return Context.RWSUsers.Where(i => i.IsConfirmed == true && EntityFunctions.TruncateTime(i.ConfirmationDate.Value) >= from);
            }
            else if (from == null && to != null)
            {
                to = to.Value.Date;
                return Context.RWSUsers.Where(i => i.IsConfirmed == true && EntityFunctions.TruncateTime(i.ConfirmationDate.Value) <= to);
            }
            else
            {
                return Context.RWSUsers.Where(i => i.IsConfirmed == true);
            }
        }

        public IQueryable<RWSUser> GetInactiveUsers(){
            try
            {
                return Context.RWSUsers.Where(i => i.IsConfirmed == false);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Get user by user Id
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <returns></returns>
        public RWSUser GetUserByUserId(int UserId)
        {
            if (Context.RWSUsers.Any(x => x.UserId == UserId))
            {
                return Context.RWSUsers.FirstOrDefault(x => x.UserId == UserId);
            }
            else
            {
                return null;
            }
        }

        //public IQueryable<Post> GetUserActivePosts(int UserId)
        //{
        //    return Context.Posts.Where(i => i.CreatedBy == UserId && i.IsActive == true);
        //}

        /// <summary>
        /// Get user posts count.
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="status">Status of the post as a string ('Active' or 'Inactive'). If value is an empty string, count of all posts will be returned</param>
        /// <returns></returns>
        public int GetUserPostsCount(int userId, string status)
        {
            if (status == "Active")
            {
                return Context.Posts.Where(i => i.CreatedBy == userId && i.IsActive == true).Count();
            }
            else if (status == "Inactive")
            {
                return Context.Posts.Where(i => i.CreatedBy == userId && (i.IsActive == null || i.IsActive.Value == false)).Count();
            }
            else
            {
                return Context.Posts.Where(i => i.CreatedBy == userId).Count();
            }
        }

        public IQueryable<Post> GetUserActivePosts(string Username, IQueryable<Post> filteredArticles)
        {
            IQueryable<Post> articles = filteredArticles.Where(i => i.IsActive == true && i.RWSUser.UserName == Username).OrderByDescending(i => i.CreationDate);
            return articles;
        }

        public void DeactivateUser(string username)
        {
            RWSUser user = Context.RWSUsers.Where(i => i.UserName == username).FirstOrDefault();
            user.IsConfirmed = false;
            Context.SaveChanges();
        }

        public void ActivateUser(string username)
        {
            RWSUser user = Context.RWSUsers.Where(i => i.UserName == username).FirstOrDefault();
            user.IsConfirmed = true;
            user.ConfirmationDate = DateTime.Now;
            Context.SaveChanges();
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        /// <param name="userName">Username</param>
        /// <returns></returns>
        public RWSUser GetUserByUsername(string userName)
        {
            if (Context.RWSUsers.Any(x => x.UserName == userName))
            {
                return Context.RWSUsers.FirstOrDefault(x => x.UserName == userName);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get all user views
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <returns></returns>
        public int GetAlltimeUserViews(int UserId)
        {
            if (Context.RWSUsers.Any(x => x.UserId == UserId))
            {
                IQueryable<Post> user_posts = Context.Posts.Where(i => i.RWSUser.UserId == UserId).Where(i => i.IsActive.Value);
                IQueryable<Point> views = Context.Points.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value)).Where(i => i.PointTypeId == 3);
                return views.Count();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get user views
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns>
        /// views count and value
        /// </returns>
        public double[] GetUserViewsByMonthId(int UserId, int MonthId, int YearId)
        {
            if (Context.RWSUsers.Any(x => x.UserId == UserId))
            {
                double[] result = new double[2];
                IQueryable<Post> user_posts = Context.Posts.Where(i => i.RWSUser.UserId == UserId).Where(i => i.IsActive.Value);
                IQueryable<Point> views = Context.Points.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value)).Where(i=> i.PointTypeId == 3).Where(i => i.PointTimestamp.Value.Month == MonthId).Where(i => i.PointTimestamp.Value.Year == YearId);
                result[0] = views.Count();
                if (result[0] > 0)
                {
                    var y = views.ToList();
                    //var x = y.Sum(i => i.PointType.PointTypeWeight.Value);
                    result[1] = views.Where(i => i.isActive.Value && i.PointType != null).Sum(i => i.PointType.PointTypeWeight.Value);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get users engagement type count (views, like, shares)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="engType"></param>
        /// <returns></returns>
        public int GetUserPointTypeCount(int userId, int pointType)
        {
            if (Context.RWSUsers.Any(i => i.UserId == userId))
            {
                return Context.PointsViews.Where(i => i.UserId == userId && i.PointTypeId == pointType).Count();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get post views
        /// </summary>
        /// <param name="postId">PostId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public double[] GetPostViewsByMonthId(int PostId, int MonthId, int YearId)
        {
            if (Context.Posts.Any(x => x.PostId == PostId))
            {
                double[] result = new double[2];
                IQueryable<Point> views = Context.Points.Where(i => i.PostId == PostId).
                    Where(i => i.PointTypeId == 3).
                    Where(i => i.PointTimestamp.Value.Month == MonthId).
                    Where(i => i.PointTimestamp.Value.Year == YearId);
                result[0] = views.Count();
                if (result[0] > 0)
                {
                    result[1] = views.Where(i => i.isActive!= null && i.isActive.Value).Sum(i => i.PointType !=null?i.PointType.PointTypeWeight.Value:0);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get user likes
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public double[] GetUserLikesByMonthId(int UserId, int MonthId, int YearId)
        {
            if (Context.RWSUsers.Any(x => x.UserId == UserId))
            {
                double[] result = new double[2];
                IQueryable<Post> user_posts = Context.Posts.Where(i => i.RWSUser.UserId == UserId).Where(i => i.IsActive.Value);
                IQueryable<Point> likes = Context.Points.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value)).Where(i => i.PointTypeId == 2).Where(i => i.PointTimestamp.Value.Month == MonthId).Where(i => i.PointTimestamp.Value.Year == YearId);
                result[0] = likes.Count();
                if (result[0] > 0)
                {
                    result[1] = likes.Where(i => i.isActive.Value).Sum(i => i.PointType.PointTypeWeight.Value);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get post likes
        /// </summary>
        /// <param name="postId">PostId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public double[] GetPostLikesByMonthId(int PostId, int MonthId, int YearId)
        {
            if (Context.Posts.Any(x => x.PostId == PostId))
            {
                double[] result = new double[2];
                IQueryable<Point> likes = Context.Points.Where(i => i.PostId == PostId).Where(i => i.PointTypeId == 2).Where(i => i.PointTimestamp.Value.Month == MonthId).Where(i => i.PointTimestamp.Value.Year == YearId);
                result[0] = likes.Count();
                if (result[0] > 0)
                {
                    result[1] = likes.Where(i => i.isActive.Value).Sum(i => i.PointType.PointTypeWeight.Value);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get user shares
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public double[] GetUserSharesByMonthId(int UserId, int MonthId, int YearId)
        {
            if (Context.RWSUsers.Any(x => x.UserId == UserId))
            {
                double[] result = new double[2];
                //RWSUser user = Context.RWSUsers.FirstOrDefault(x => x.UserId == UserId);
                IQueryable<Post> user_posts = Context.Posts.Where(i => i.RWSUser.UserId == UserId).Where(i => i.IsActive.Value);
                IQueryable<Point> shares = Context.Points.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value)).Where(i => i.PointTypeId == 1).Where(i => i.PointTimestamp.Value.Month == MonthId).Where(i => i.PointTimestamp.Value.Year == YearId);
                result[0] = shares.Count();
                if (result[0] > 0)
                {
                    result[1] = shares.Where(i => i.isActive.Value).Sum(i => i.PointType.PointTypeWeight.Value);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get post shares
        /// </summary>
        /// <param name="postId">PostId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public double[] GetPostSharesByMonthId(int PostId, int MonthId, int YearId)
        {
            if (Context.Posts.Any(x => x.PostId == PostId))
            {
                double[] result = new double[2];
                IQueryable<Point> shares = Context.Points.Where(i => i.PostId == PostId).Where(i => i.PointTypeId == 1).Where(i => i.PointTimestamp.Value.Month == MonthId).Where(i => i.PointTimestamp.Value.Year == YearId);
                result[0] = shares.Count();
                if (result[0] > 0)
                {
                    result[1] = shares.Where(i => i.isActive.Value).Sum(i => i.PointType.PointTypeWeight.Value);
                }
                return result;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get user points by month number
        /// </summary>
        /// <param name="userName">Username</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public double GetUserPointsByMonthId(int userId, int monthId, int yearId)
        {
            if (Context.RWSUsers.Any(x => x.UserId == userId))
            {
                IQueryable<PointsView> pointsview = Context.PointsViews.Where(i => i.UserId == userId && i.isActive == true && i.PointTimestamp.Value.Month == monthId && i.PointTimestamp.Value.Year == yearId);
                if (pointsview.Count() > 0)
                {
                    return pointsview.Sum(i => i.PointTypeWeight.Value);
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get user points by selected date. If date values are null, all time points are returned
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="startDate">start Date</param>
        /// <param name="endDate">end Date</param>
        /// <returns></returns>
        public double GetUserPointsBySelectedDate(int userId, DateTime? startDate, DateTime? endDate)
        {
            if (Context.RWSUsers.Any(x => x.UserId == userId))
            {
                if (startDate != null && endDate == null)
                {
                    endDate = DateTime.Now;
                }
                if (startDate == null && endDate == null)
                {
                    IQueryable<PointsView> pointsview = Context.PointsViews.Where(i => i.UserId == userId && i.isActive == true);
                    if (pointsview.Count() > 0)
                    {
                        return pointsview.Sum(i => i.PointTypeWeight.Value);
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    startDate = startDate.Value.Date;
                    endDate = endDate.Value.Date;
                    IQueryable<PointsView> pointsview = Context.PointsViews.Where(i => i.UserId == userId && i.isActive == true && EntityFunctions.TruncateTime(i.PointTimestamp.Value) >= startDate && EntityFunctions.TruncateTime(i.PointTimestamp.Value) <= endDate);
                    if (pointsview.Count() > 0)
                    {
                        return pointsview.Sum(i => i.PointTypeWeight.Value);
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get user by email
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns></returns>
        public RWSUser GetUserByEmail(string email)
        {
            if (Context.RWSUsers.Any(x => x.Email == email))
            {
                return Context.RWSUsers.FirstOrDefault(x => x.Email == email);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get user by confirmation token
        /// </summary>
        /// <param name="confirmationtoken">Confirmationtoken</param>
        /// <returns></returns>
        public RWSUser GetUserByConfirmationToken(string confirmationtoken)
        {
            if (Context.RWSUsers.Any(x => x.ConfirmationToken == confirmationtoken))
            {
                return Context.RWSUsers.FirstOrDefault(x => x.ConfirmationToken == confirmationtoken);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get user by password token
        /// </summary>
        /// <param name="passwordtoken">Confirmationtoken</param>
        /// <returns></returns>
        public RWSUser GetUserByPasswordToken(string passwordtoken)
        {
            if (Context.RWSUsers.Any(x => x.PasswordVerificationToken == passwordtoken))
            {
                return Context.RWSUsers.FirstOrDefault(x => x.PasswordVerificationToken == passwordtoken);
            }
            else
            {
                return null;
            }
        }

        public static string GetMd5Hash(string value)
        {
            var md5Hasher = MD5.Create();
            var data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(value));
            var sBuilder = new System.Text.StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        /// <summary>
        /// Gets the user with that username and password
        /// </summary>
        /// <param name="userName">UserName</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public RWSUser GetUserByUsernameAndPassword(string userName, string password)
        {
            string pass = GetMd5Hash(password);
            if (Context.RWSUsers.Any(x => x.UserName == userName && x.Password == pass))
            {
                return Context.RWSUsers.FirstOrDefault(x => x.UserName == userName && x.Password == pass);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="userName">Username</param>
        /// <param name="newPassword">The new password</param>
        /// <returns></returns>
        public bool ChangeUserPassword(string userName, string newPassword)
        {
            try
            {
                var user = Context.RWSUsers.FirstOrDefault(i => i.UserName == userName);
                user.Password = newPassword;
                user.PasswordChangedDate = DateTime.Now;
                Context.Entry(user).State = System.Data.EntityState.Modified;
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a user with that email exists
        /// </summary>
        /// <param name="email">email</param>
        /// <returns></returns>
        public bool IsValidEmail(string email)
        {
            if (Context.RWSUsers.Any(x => x.Email == email))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a user with that username and password exists
        /// </summary>
        /// <param name="userName">Username</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        public bool IsValid(string userName, string password)
        {
            if (Context.RWSUsers.Any(x => x.UserName == userName && x.Password == password))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if a username already exists
        /// </summary>
        /// <param name="userName">Username</param>
        /// <returns></returns>
        public bool CheckIfUsernameExist(string userName)
        {
            if (Context.RWSUsers.Any(x => x.UserName == userName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if an email already exists
        /// </summary>
        /// <param name="email">Email</param>
        /// <returns></returns>
        public bool CheckIfEmailExists(string email)
        {
            if (Context.RWSUsers.Any(x => x.Email == email))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool UserExists(string username, string password)
        {
            password = GetMd5Hash(password);
            if (Context.RWSUsers.Any(x => x.UserName == username && x.Password == password))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ExternalUserExists(string id, string provider)
        {
            if (Context.RWSProviderUsers.Any(x => x.ProviderUserId == id && x.Provider == provider))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public RWSUser GetExternalUser(string id, string provider)
        {
            return Context.RWSProviderUsers.FirstOrDefault(i => i.ProviderUserId == id && i.Provider == provider).RWSUser;
        }

        public void CreateOrUpdateExternalUser(string id, string provider)
        {
            if (!ExternalUserExists(id, provider))
            {
                RWSProviderUser user = new RWSProviderUser();
                user.ProviderUserId = id;
                user.Provider = provider;
                Context.RWSProviderUsers.Add(user);
                Context.SaveChanges();
            }
        }

        public bool DeleteUser(RWSUser user)
        {
            try
            {
                Context.RWSUsers.Remove(user);
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddUser(RWSUser user)
        {
            try
            {
                user.CreationDate = DateTime.Now;
                user.ConfirmationToken = new Guid().ToString();
                user.IsBanned = false;
                //user.RWSRoles.Add(GetRoleByName("User"));
                Context.RWSUsers.Add(user);
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool AddProviderUser(RWSProviderUser user)
        {
            try
            {
                Context.RWSProviderUsers.Add(user);
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool ConfirmUser(RWSUser user)
        {
            try
            {
                user.IsConfirmed = true;
                Context.Entry(user).State = System.Data.EntityState.Modified;
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateProfileImg(RWSUser user, string path)
        {
            try
            {
                user.ProfileImagePath = path;
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateUserDetails(RWSUser user)
        {
            try
            {
                //string userHashedPassword = GetUserByUserId(user.UserId).Password;
                //if (user.Password != userHashedPassword)
                //{
                //    user.Password = GetMd5Hash(user.Password);
                //}
                Context.Entry(user).State = System.Data.EntityState.Modified;
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool UpdateUserDetails(IUserProfile user)
        {
            RWSUser rws_user = GetUserByUsername(user.UserName);
            rws_user.Twitter = user.Twitter;
            rws_user.Google = user.Google;
            rws_user.DisplayName = user.DisplayName;
            rws_user.FavTeam = user.FavTeamId;
            rws_user.FavComp = user.FavCompId;
            rws_user.AboutYou = user.AboutYou;

            try
            {
                Context.Entry(rws_user).State = System.Data.EntityState.Modified;
                Context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region Roles
        public void AddUserToRoles(string[] users, string[] roles)
        {
            foreach (var item in users)
            {
                RWSUser user = GetUserByUsername(item);
                if (user != null)
                {
                    foreach (var role in roles)
                    {
                        if (!IsUserInRole(user, role))
                            AddUserToRole(user.UserName, role);
                    }
                }
            }
        }

        public void AddUserToRole(string username, string role)
        {
            RWSUser userObj = GetUserByUsername(username);
            Context.RWSRoles.FirstOrDefault(r => r.RoleName == role).RWSUsers.Add(userObj);
            Context.SaveChanges();
        }

        public void AddRole(string roleName)
        {
            Context.RWSRoles.Add(new RWSRole { RoleName = roleName });
            Context.SaveChanges();
        }

        public void DeleteRole(string roleName)
        {
            Context.RWSRoles.Remove(GetRoleByName(roleName));
            Context.SaveChanges();
        }

        public void DeleteUserRoles(int UserId)
        {
            RWSUser u = GetUserByUserId(UserId);
            u.RWSRoles.Clear();
            //Context.RWSUsers.Where(i => i.UserId == UserId).Select(i => i.RWSRoles).ToList().Clear();
            Context.SaveChanges();
        }

        public bool IsUserInRole(RWSUser user, string role)
        {
            return Context.RWSRoles.Any(i => i.RWSUsers.Contains(user));
        }

        public RWSRole GetRoleByName(string role)
        {
            RWSRole userRole = Context.RWSRoles.FirstOrDefault(i => i.RoleName == role);
            if (userRole == null)
            {
                userRole = Context.RWSRoles.Add(new RWSRole { RoleName = role });
                Context.SaveChanges();
            }
            return userRole;
        }

        public bool RoleExists(string roleName)
        {
            return Context.RWSRoles.Any(r=> r.RoleName == roleName);
        }

        public IEnumerable<string> GetRolesForUser(string username)
        {
            RWSUser user = GetUserByUsername(username);

            return user.RWSRoles.Select(i => i.RoleName);
        }

        public IQueryable<string> GetAllRoles()
        {
            return Context.RWSRoles.Select(i => i.RoleName);
        }

        public IQueryable<RWSRole> GetRoles()
        {
            return Context.RWSRoles;
        }
        #endregion
    }
}
