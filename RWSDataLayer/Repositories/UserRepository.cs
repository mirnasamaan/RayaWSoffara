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

namespace RWSDataLayer.Repositories
{
    public class UserRepository : BaseRepository<RWSUser>
    {
        #region Regular Users
        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        public IQueryable<RWSUser> GetAllUsers()
        {
            return Context.RWSUsers;
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
                leaderIds = Context.UserPointsViews
                    .GroupBy(i => new { i.UserId })
                    .OrderByDescending(sum => sum.Sum(i => i.EngWeight.Value))
                    .Select(i => i.Key.UserId)
                    .Take(count).ToList();
            }
            else
            {
                leaderIds = Context.UserPointsViews
                    .Where(i => i.EngTimestamp.Value >= startDate && i.EngTimestamp.Value <= endDate)
                    .GroupBy(i => new { i.UserId })
                    .OrderByDescending(sum => sum.Sum(i => i.EngWeight.Value))
                    .Select(i => i.Key.UserId)
                    .Take(count).ToList();
            }

            return leaderIds;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns></returns>
        public IQueryable<RWSUser> GetAllUsers(int page, int size, out int totalNo)
        {
            totalNo = Context.RWSUsers.Count();
            int noOfItems = page * size;
            return Context.RWSUsers.OrderBy(i => i.UserName).Skip(noOfItems).Take(size);
        }

        public IQueryable<RWSUser> GetUsersBySearchTerm(int startIndex, int count, string term)
        {
            if (startIndex > Context.RWSUsers.Count())
                return null;
            else
                return Context.RWSUsers.Where(i => i.UserName.Contains(term)).OrderByDescending(i => i.UserName).Skip(startIndex).Take(count);
        }

        public IQueryable<RWSUser> GetUsersByActivationDate(DateTime day)
        {
            try
            {
                return Context.RWSUsers.Where(i => i.ConfirmationDate == day);
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

        /// <summary>
        /// Get all active users
        /// </summary>
        /// <returns></returns>
        public IQueryable<RWSUser> GetAllActiveUsers()
        {
            return Context.RWSUsers.Where(i => i.IsConfirmed == true);
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

        public IQueryable<Post> GetUserActivePosts(int UserId)
        {
            return Context.Posts.Where(i => i.CreatedBy == UserId && i.IsActive == true);
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
        /// Get user views
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public int GetUserViewsByMonthId(int UserId, int MonthId, int YearId)
        {
            if (Context.RWSUsers.Any(x => x.UserId == UserId))
            {
                //RWSUser user = Context.RWSUsers.FirstOrDefault(x => x.UserId == UserId);
                IQueryable<Post> user_posts = Context.Posts.Where(i => i.RWSUser.UserId == UserId).Where(i => i.IsActive.Value);
                IQueryable<Engagement> views = Context.Engagements.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value)).Where(i=> i.EngTypeId == 3).Where(i => i.EngTimestamp.Value.Month == MonthId).Where(i => i.EngTimestamp.Value.Year == YearId);
                return views.Count();
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
        public int GetPostViewsByMonthId(int PostId, int MonthId, int YearId)
        {
            if (Context.Posts.Any(x => x.PostId == PostId))
            {
                IQueryable<Engagement> views = Context.Engagements.Where(i => i.PostId == PostId).Where(i => i.EngTypeId == 3).Where(i => i.EngTimestamp.Value.Month == MonthId).Where(i => i.EngTimestamp.Value.Year == YearId);
                return views.Count();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get user likes
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public int GetUserLikesByMonthId(int UserId, int MonthId, int YearId)
        {
            if (Context.RWSUsers.Any(x => x.UserId == UserId))
            {
                //RWSUser user = Context.RWSUsers.FirstOrDefault(x => x.UserId == UserId);
                IQueryable<Post> user_posts = Context.Posts.Where(i => i.RWSUser.UserId == UserId).Where(i => i.IsActive.Value);
                IQueryable<Engagement> likes = Context.Engagements.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value)).Where(i => i.EngTypeId == 2).Where(i => i.EngTimestamp.Value.Month == MonthId).Where(i => i.EngTimestamp.Value.Year == YearId);
                return likes.Count();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get post likes
        /// </summary>
        /// <param name="postId">PostId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public int GetPostLikesByMonthId(int PostId, int MonthId, int YearId)
        {
            if (Context.Posts.Any(x => x.PostId == PostId))
            {
                IQueryable<Engagement> likes = Context.Engagements.Where(i => i.PostId == PostId).Where(i => i.EngTypeId == 2).Where(i => i.EngTimestamp.Value.Month == MonthId).Where(i => i.EngTimestamp.Value.Year == YearId);
                return likes.Count();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get user shares
        /// </summary>
        /// <param name="userId">UserId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public int GetUserSharesByMonthId(int UserId, int MonthId, int YearId)
        {
            if (Context.RWSUsers.Any(x => x.UserId == UserId))
            {
                //RWSUser user = Context.RWSUsers.FirstOrDefault(x => x.UserId == UserId);
                IQueryable<Post> user_posts = Context.Posts.Where(i => i.RWSUser.UserId == UserId).Where(i => i.IsActive.Value);
                IQueryable<Engagement> shares = Context.Engagements.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value)).Where(i => i.EngTypeId == 1).Where(i => i.EngTimestamp.Value.Month == MonthId).Where(i => i.EngTimestamp.Value.Year == YearId);
                return shares.Count();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get post shares
        /// </summary>
        /// <param name="postId">PostId</param>
        /// <param name="monthId">Month</param>
        /// <param name="yearId">Year</param>
        /// <returns></returns>
        public int GetPostSharesByMonthId(int PostId, int MonthId, int YearId)
        {
            if (Context.Posts.Any(x => x.PostId == PostId))
            {
                IQueryable<Engagement> shares = Context.Engagements.Where(i => i.PostId == PostId).Where(i => i.EngTypeId == 1).Where(i => i.EngTimestamp.Value.Month == MonthId).Where(i => i.EngTimestamp.Value.Year == YearId);
                return shares.Count();
            }
            else
            {
                return 0;
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
                //RWSUser user = Context.RWSUsers.FirstOrDefault(x => x.UserId == userId);
                IQueryable<Post> user_posts = Context.Posts.Where(i => i.RWSUser.UserId == userId).Where(i => i.IsActive.Value);
                IQueryable<Engagement> engagements = Context.Engagements.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value)).Where(i => i.EngTimestamp.Value.Month == monthId).Where(i => i.EngTimestamp.Value.Year == yearId);

                double points = 0;
                foreach (var item in engagements)
                {
                    points += item.EngagementType.EngWeight.Value;
                }
                return points;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Get user points by month number
        /// </summary>
        /// <param name="userId">User Id</param>
        /// <param name="startDate">start Date</param>
        /// <param name="endDate">end Date</param>
        /// <returns></returns>
        public double GetUserPointsBySelectedDate(int userId, DateTime? startDate, DateTime? endDate)
        {
            if (Context.RWSUsers.Any(x => x.UserId == userId))
            {
                RWSUser user = Context.RWSUsers.FirstOrDefault(x => x.UserId == userId);
                IQueryable<Post> user_posts = Context.Posts.Where(i => i.RWSUser.UserId == user.UserId).Where(i => i.IsActive.Value);
                IQueryable<Engagement> engagements;
                if (startDate != null && endDate == null)
                {
                    endDate = DateTime.Now;
                }
                if (startDate == null && endDate == null)
                {
                    engagements = Context.Engagements.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value));
                }
                else
                {
                    engagements = Context.Engagements.Where(i => user_posts.Select(j => j.PostId).Contains(i.PostId.Value)).Where(i => i.EngTimestamp.Value >= startDate).Where(i => i.EngTimestamp.Value <= endDate);
                }
                double points = 0;
                foreach (var item in engagements)
                {
                    points += item.EngagementType.EngWeight.Value;
                }
                return points;
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
                user.IsConfirmed = false;
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
