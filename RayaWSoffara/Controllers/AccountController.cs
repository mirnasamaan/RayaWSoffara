using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using RayaWSoffara.Filters;
using RWSDataLayer;
using RayaWSoffara.Models;
using Postal;
using System.Net;
using RWSDataLayer.Repositories;
using System.Web.Routing;
using RWSInfrastructure.Services;
using RWSDataLayer.Context;
using RWSInfrastructure.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using RayaWSoffara.Helpers;


namespace RayaWSoffara.Controllers
{
    [Authorize]
    public class AccountController : SearchController
    {
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }
        private UserRepository _userRepo;
        private EngagementRepository _engRepo = new EngagementRepository();

        protected override void Initialize(RequestContext requestContext)
        {
            if (FormsService == null) { FormsService = new FormsAuthenticationService(); }
            if (MembershipService == null) { MembershipService = new AccountMembershipService(); }
            _userRepo = new UserRepository();

            base.Initialize(requestContext);
        }

        [HttpPost]
        [AllowAnonymous]
        public bool DoesUserEmailExist(string Email)
        {
            var email = _userRepo.IsValidEmail(Email);

            return (email);
        }

        [HttpPost]
        [AllowAnonymous]
        public bool DoesUserNameExist(string UserName)
        {
            var user = _userRepo.GetUserByUsername(UserName);

            return (user == null);
        }

        [AllowAnonymous]
        public ActionResult Login(string RedirectUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (RedirectUrl == "")
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return Redirect(RedirectUrl);
                }
            }
            else
            {
                ViewBag.ReturnUrl = RedirectUrl;
                return View();
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)]
        public ActionResult Login(LoginModel model, string RedirectUrl)
        {
            if (ModelState.IsValid)
            {
                if (MembershipService.ValidateUser(model.UserName, model.Password))
                {
                    bool confirmed = _userRepo.GetUserByUsernameAndPassword(model.UserName, model.Password).IsConfirmed.Value;
                    if (confirmed == false)
                    {
                        ModelState.AddModelError("", "أسم المستخدم أو كلمة السر غير صحيح.");
                        return View(model);
                    }
                    FormsService.SignIn(model.UserName, model.RememberMe);
                    //if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    //    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    if(RedirectUrl != "")
                    {
                        return Redirect(RedirectUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "أسم المستخدم أو كلمة السر غير صحيح.");
            }
            return View(model);
        }

        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsService.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            ViewBag.Countries = GetAllCountries();
            return View();
        }

        public List<string> GetAllCountries()
        {
            List<string> cultureList = new List<string>();
            
            foreach (var cultureInfo in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                RegionInfo region = new RegionInfo(cultureInfo.LCID);

                if (!(cultureList.Contains(region.EnglishName)))
                {
                    cultureList.Add(region.EnglishName);
                }
            }
            cultureList.Sort();
            return cultureList;
        }

        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult ForgotPassword(string Email)
        {
            RWSUser user = _userRepo.GetUserByEmail(Email);
            if (user != null)
            {
                dynamic email = new Email("ChngPasswordEmail");
                email.To = Email;
                email.UserName = user.UserName;
                email.ConfirmationToken = user.PasswordVerificationToken;
                email.BaseUrl = Request.Url.Authority;
                email.Send();
                ViewBag.Message = "لقد تم ارسال رسالة الى بريدك الالكتروني";
                return RedirectToAction("ConfirmationEmailSent", "Account", new { message = "لقد تم ارسال رسالة الى بريدك الالكتروني" });
            }
            ViewBag.ErrorMsg = "البريد الالكتروني غير مسجل.";
            return View();
        }

        [AllowAnonymous]
        public ActionResult NewPassword(string Id)
        {
            ViewBag.PasswordToken = Id;
            return View();
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

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult NewPassword(string password, string Id)
        {
            RWSUser user = _userRepo.GetUserByPasswordToken(Id);
            if (user != null)
            {
                user.Password = password;
                user.Password = GetMd5Hash(password);
                _userRepo.UpdateUserDetails(user);
                ViewBag.Message = "تم تغيير كلمة المرور.";
                return RedirectToAction("ConfirmationEmailSent", "Account", new { message = "تم تغيير كلمة المرور." });
            }
            //ViewBag.ErrorMsg = "البريد الالكتروني غير مسجل.";
            return View();
        }

        [AllowAnonymous]
        public ActionResult ConfirmationEmailSent(string message)
        {
            ViewBag.Message = message;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model, string sender)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string remoteip = Request.UserHostAddress;
                    string recaptcha = Request.Form["g-recaptcha-response"];
                    bool valid = CaptchaHelper.ValidateCaptcha("6LdhiRQTAAAAAMRMQP5NdFFtj2pgyAZljMcs1nAe", recaptcha, remoteip);
                    if (valid)
                    {
                        var createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);

                        if (createStatus == MembershipCreateStatus.Success)
                        {
                            RWSUser user = _userRepo.GetUserByUsername(model.UserName);
                            user.FirstName = model.FirstName;
                            user.LastName = model.LastName;
                            user.Country = model.Country;
                            user.ConfirmationToken = Guid.NewGuid().ToString();
                            user.IsConfirmed = false;
                            user.PasswordVerificationToken = Guid.NewGuid().ToString();
                            user.RWSRoles.Add(_userRepo.GetRoleByName("User"));
                            _userRepo.UpdateUserDetails(user);
                            dynamic email = new Email("RegEmail");
                            email.To = model.Email;
                            email.UserName = model.UserName;
                            email.ConfirmationToken = user.ConfirmationToken;
                            email.BaseUrl = Request.Url.Authority;
                            try
                            {
                                email.Send();
                                if (sender != null && sender != "")
                                {
                                    _engRepo.AddInvitationPoints(sender);
                                }
                                return RedirectToAction("ConfirmationEmailSent", "Account", new { message = "لقد تم ارسال رسالة الى بريدك الالكتروني" });
                            }
                            catch (Exception ex)
                            {
                                return RedirectToAction("ConfirmationFailure", "Account", new { message = "العملية لم تتم بنجاح" });
                            }
                        }
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Countries = GetAllCountries();
            return View(model);
        }

        public static string GetFacebookPictureUrl(string FaceBookUserId)
        {
            WebResponse response = null;
            string pictureUrl = string.Empty;
            try
            {
                WebRequest request = WebRequest.Create(string.Format("http://graph.facebook.com/" + FaceBookUserId + "/picture?type=large"));
                response = request.GetResponse();
                pictureUrl = response.ResponseUri.ToString();
            }
            catch (Exception ex)
            {
                //? handle
            }
            finally
            {
                if (response != null) response.Close();
            }
            return pictureUrl;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] 
        public ActionResult Register2(RegisterExternalLoginModel model, string returnUrl, string provider, string providerUserId)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    string remoteip = Request.UserHostAddress;
                    string recaptcha = Request.Form["g-recaptcha-response"];
                    bool valid = CaptchaHelper.ValidateCaptcha("6LdhiRQTAAAAAMRMQP5NdFFtj2pgyAZljMcs1nAe", recaptcha, remoteip);
                    if (valid)
                    {
                        var createStatus = MembershipService.CreateUser(model.UserName, model.Password, model.Email);

                        if (createStatus == MembershipCreateStatus.Success)
                        {
                            RWSUser user = _userRepo.GetUserByUsername(model.UserName);
                            user.FirstName = model.FirstName;
                            user.LastName = model.LastName;
                            user.Country = model.Country;
                            user.PasswordVerificationToken = Guid.NewGuid().ToString();
                            user.IsConfirmed = true;
                            user.ConfirmationDate = DateTime.UtcNow.ToLocalTime();
                            user.ProfileImagePath = "/" + model.PicturePath;
                            _userRepo.UpdateUserDetails(user);

                            string FacebookPictureUrl = GetFacebookPictureUrl(providerUserId);
                            WebClient webClient = new WebClient();
                            webClient.DownloadFile(FacebookPictureUrl, @AppDomain.CurrentDomain.BaseDirectory + model.PicturePath);

                            RWSProviderUser provider_user = new RWSProviderUser();
                            provider_user.UserId = user.UserId;
                            provider_user.Provider = provider;
                            provider_user.ProviderUserId = providerUserId;
                            _userRepo.AddProviderUser(provider_user);
                            //dynamic email = new Email("RegEmail");
                            //email.To = model.Email;
                            //email.UserName = model.UserName;
                            //email.ConfirmationToken = user.ConfirmationToken;
                            //email.Send();

                            return RedirectToAction("Index", "Home");
                        }
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult RegisterConfirmation(string Id)
        {
            RWSUser user = _userRepo.GetUserByConfirmationToken(Id);
            if (_userRepo.ConfirmUser(user))
            {
                user.ConfirmationDate = DateTime.UtcNow.ToLocalTime();
                user.IsConfirmed = true;
                _userRepo.UpdateUserDetails(user);
                FormsService.SignIn(user.UserName, false);
                return RedirectToAction("ConfirmationSuccess");
            }
            return RedirectToAction("ConfirmationFailure");
        }

        [AllowAnonymous]
        public ActionResult ConfirmationSuccess()
        {
            return View();
        }
 
        [AllowAnonymous]
        public ActionResult ConfirmationFailure()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        [AllowAnonymous]
        public ActionResult Profile(UserProfileVM rws_user)
        {
            _userRepo.UpdateUserDetails(rws_user);
            //return Profile(rws_user.UserName);
            return RedirectToAction("Profile", new { Username = rws_user.UserName, Page = 1 });
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Profile(string Username, int Page)
        {
            CompetitionRepository _compRepo = new CompetitionRepository();
            UserProfileVM userProfile = new UserProfileVM();

            RWSUser user = _userRepo.GetUserByUsername(Username);
            userProfile.UserId = user.UserId;
            userProfile.UserName = user.UserName;
            userProfile.FirstName = user.FirstName;
            userProfile.LastName = user.LastName;
            userProfile.articlesCount = _userRepo.GetUserPostsCount(user.UserId, "Active"); //user.Posts.Where(i => i.IsActive == true).Count();
            userProfile.profileImgUrl = user.ProfileImagePath;
            userProfile.DisplayName = user.DisplayName;
            if(user.FavComp != null){
                userProfile.FavCompId = user.FavComp.Value;
                userProfile.FavCompName = _compRepo.GetCompetetionById(user.FavComp.Value).CompetitionName;
            }
            if (user.FavTeam != null)
            {
                userProfile.FavTeamId = user.FavTeam.Value;
                userProfile.FavTeamName = _compRepo.GetTeamById(user.FavTeam.Value).TeamName;
            }

            ArticleRepository _articleRepo = new ArticleRepository();
            userProfile.recentArticles = _articleRepo.GetRecentArticlesByUserId(user.UserId).ToList();
            userProfile.viewsCount = _engRepo.GetEngCountByUserId(user.UserId, 3);

            List<UserPointsVM> Points = new List<UserPointsVM>();
            Points = GetUserAchievements(user.UserId, Page);
            ViewBag.TotalPoints = Points; 
            
            int membershipMonthCount = Math.Abs((user.ConfirmationDate.Value.Year * 12 + user.ConfirmationDate.Value.Month) - (DateTime.UtcNow.ToLocalTime().Year * 12 + DateTime.UtcNow.ToLocalTime().Month)) + 1;
            ViewBag.AllPointsCount = membershipMonthCount;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_AchievementsPartial", userProfile);
            }

            return View(userProfile);
        }

        public List<UserPointsVM> GetUserAchievements(int UserId, int Page)
        {
            RWSUser user = _userRepo.GetUserByUserId(UserId);
            //int membershipMonthCount = Math.Abs((user.ConfirmationDate.Value.Year * 12 + user.ConfirmationDate.Value.Month) - (DateTime.UtcNow.ToLocalTime().Year * 12 + DateTime.UtcNow.ToLocalTime().Month)) + 1;

            List<UserPointsVM> Points = new List<UserPointsVM>();
            DateTime startDate = DateTime.UtcNow.ToLocalTime().AddMonths(-(Page-1)*6);
            //int monthId = DateTime.UtcNow.ToLocalTime().Month;
            //int yearId = DateTime.UtcNow.ToLocalTime().Year;
            int monthId = startDate.Month;
            int yearId = startDate.Year;
            for (int i = 0; i < 6; i++)
            {
                UserPointsVM points = new UserPointsVM();
                points.UserId = UserId;
                points.UserName = user.UserName;
                //double[] views = _userRepo.GetUserViewsByMonthId(UserId, monthId, yearId);
                //double[] likes = _userRepo.GetUserLikesByMonthId(UserId, monthId, yearId);
                //double[] shares = _userRepo.GetUserSharesByMonthId(UserId, monthId, yearId);
                int viewsCount = _userRepo.GetUserPointTypeCount(UserId, 3);
                int likesCount = _userRepo.GetUserPointTypeCount(UserId, 2);
                int sharesCount = _userRepo.GetUserPointTypeCount(UserId, 1);
                points.ViewsCount = viewsCount;
                points.LikesCount = likesCount;
                points.SharesCount = sharesCount;
                points.MonthId = monthId;
                switch (monthId)
                {
                    case 1: points.MonthName = "يناير"; break;
                    case 2: points.MonthName = "فبراير"; break;
                    case 3: points.MonthName = "مارس"; break;
                    case 4: points.MonthName = "ابريل"; break;
                    case 5: points.MonthName = "مايو"; break;
                    case 6: points.MonthName = "يونيو"; break;
                    case 7: points.MonthName = "يوليو"; break;
                    case 8: points.MonthName = "أغسطس"; break;
                    case 9: points.MonthName = "سبتمبر"; break;
                    case 10: points.MonthName = "أكتوبر"; break;
                    case 11: points.MonthName = "نوفمبر"; break;
                    case 12: points.MonthName = "ديسمبر"; break;
                    default: points.MonthName = ""; break;
                }
                points.YearId = yearId;
                points.PointsValue = _userRepo.GetUserPointsByMonthId(UserId, monthId, yearId);
                Points.Add(points);

                DateTime currentDate = new DateTime(yearId, monthId, 1);
                if (currentDate <= user.ConfirmationDate.Value) break;
                monthId--;
                if (monthId == 0)
                {
                    monthId = 12;
                    yearId--;
                }

            }
            return Points;
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Points(string Username, int MonthId, int YearId)
        {
            ArticleRepository _articleRepo = new ArticleRepository();
            CompetitionRepository _compRepo = new CompetitionRepository();
            UserProfileVM userProfile = new UserProfileVM();

            RWSUser user = _userRepo.GetUserByUsername(Username);
            userProfile.UserId = user.UserId;
            userProfile.UserName = user.UserName;
            userProfile.FirstName = user.FirstName;
            userProfile.LastName = user.LastName;
            userProfile.articlesCount = _userRepo.GetUserPostsCount(user.UserId, "Active");
            userProfile.profileImgUrl = user.ProfileImagePath;
            userProfile.DisplayName = user.DisplayName;

            userProfile.viewsCount = _userRepo.GetAlltimeUserViews(user.UserId);

            UserPointsDetailsVM points = new UserPointsDetailsVM();
            points.UserId = userProfile.UserId;
            points.UserName = Username;
            points.MonthId = MonthId;
            switch (MonthId)
            {
                case 1: points.MonthName = "يناير"; break;
                case 2: points.MonthName = "فبراير"; break;
                case 3: points.MonthName = "مارس"; break;
                case 4: points.MonthName = "ابريل"; break;
                case 5: points.MonthName = "مايو"; break;
                case 6: points.MonthName = "يونيو"; break;
                case 7: points.MonthName = "يوليو"; break;
                case 8: points.MonthName = "أغسطس"; break;
                case 9: points.MonthName = "سبتمبر"; break;
                case 10: points.MonthName = "أكتوبر"; break;
                case 11: points.MonthName = "نوفمبر"; break;
                case 12: points.MonthName = "ديسمبر"; break;
                default: points.MonthName = ""; break;
            }
            points.YearId = YearId;
            points.TotalPointsValue = _userRepo.GetUserPointsByMonthId(userProfile.UserId, MonthId, YearId);
            points.PostsPoints = new List<PostPointsVM>();

            IQueryable<Post> posts = _articleRepo.GetUserPostsWithMonthId(userProfile.UserId, MonthId, YearId, 0);
            ViewBag.AllPostsCount = _articleRepo.GetUserPostsCountWithMonthId(userProfile.UserId, MonthId, YearId);

            foreach (var item in posts)
            {
                PostPointsVM post_points = new PostPointsVM();
                post_points.PostId = item.PostId;
                post_points.PostTitle = item.Title;
                post_points.PostFeaturedImage = item.FeaturedImage;
                post_points.PostFeaturedVideo = item.FeaturedVideo;
                post_points.PostContent = item.Content;

                double[] shares = _userRepo.GetPostSharesByMonthId(item.PostId, MonthId, YearId);
                double[] likes = _userRepo.GetPostLikesByMonthId(item.PostId, MonthId, YearId);
                double[] views = _userRepo.GetPostViewsByMonthId(item.PostId, MonthId, YearId);
                post_points.PostSharesCount = Convert.ToInt32(shares[0]);
                post_points.PostSharesValue = shares[1];
                post_points.PostLikesCount = Convert.ToInt32(likes[0]);
                post_points.PostLikesValue = likes[1];
                post_points.PostViewsCount = Convert.ToInt32(views[0]);
                post_points.PostViewsValue = views[1];
                points.PostsPoints.Add(post_points);
            }

            ViewBag.PointsDetails = points;
            ViewBag.EngagementTypes = _compRepo.GetAllEngagementTypesWithWeight();

            return View(userProfile);
        }

        [HttpPost]
        public ActionResult PointsPartial(int UserId, int MonthId, int YearId, int Page)
        {
            ArticleRepository _articleRepo = new ArticleRepository();
            CompetitionRepository _compRepo = new CompetitionRepository();
            IQueryable<Post> posts = _articleRepo.GetUserPostsWithMonthId(UserId, MonthId, YearId, Page);

            List<PostPointsVM> postsPoints = new List<PostPointsVM>();
            foreach (var item in posts)
            {
                PostPointsVM post_points = new PostPointsVM();
                post_points.PostId = item.PostId;
                post_points.PostTitle = item.Title;
                post_points.PostFeaturedImage = item.FeaturedImage;
                post_points.PostFeaturedVideo = item.FeaturedVideo;
                post_points.PostContent = item.Content;

                double[] shares = _userRepo.GetPostSharesByMonthId(item.PostId, MonthId, YearId);
                double[] likes = _userRepo.GetPostLikesByMonthId(item.PostId, MonthId, YearId);
                double[] views = _userRepo.GetPostViewsByMonthId(item.PostId, MonthId, YearId);
                post_points.PostSharesCount = Convert.ToInt32(shares[0]);
                post_points.PostSharesValue = shares[1];
                post_points.PostLikesCount = Convert.ToInt32(likes[0]);
                post_points.PostLikesValue = likes[1];
                post_points.PostViewsCount = Convert.ToInt32(views[0]);
                post_points.PostViewsValue = views[1];
                postsPoints.Add(post_points);
            }
            ViewBag.EngagementTypes = _compRepo.GetAllEngagementTypesWithWeight();
            return PartialView("_PointsPartial", postsPoints);
        }

        public class GraphData
        {
          public double Value { get; set; }
          public long Date { get; set; }
        }

        public static long GetJavascriptTimestamp(DateTime input)
        {
            TimeSpan span = new System.TimeSpan(DateTime.Parse("1/1/1970").Ticks);
            DateTime time = input.Subtract(span);
            return (long)(time.Ticks / 10000);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult GetGraphPoints(int userId)
        {
            int currMonth = DateTime.UtcNow.ToLocalTime().Month;
            int currYear = DateTime.UtcNow.ToLocalTime().Year;
            int beginMonth, beginYear;

            beginMonth = currMonth - 6 + 1;
            beginYear = currYear;
            if (beginMonth < 1)
            {
                beginMonth += 12;
                beginYear--;
            }

            double[][] points = new double[6][];
            for (int i = 0; i < 6; i++)
            {
                int month = beginMonth + i;
                int year = beginYear;
                if (month > 12)
                {
                    month -= 12;
                    year = beginYear + 1;
                }

                points[i] = new double[2];
                points[i][0] = GetJavascriptTimestamp(new DateTime(year, month, 1));
                points[i][1] = _userRepo.GetUserPointsByMonthId(userId, month, year);
            }

            return Json(points, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpGet]
        public ActionResult Settings(string Username)
        {
            if (User.Identity.Name == "")
            {
                return Redirect("/Login?RedirectUrl=/Settings?Username=" + Username);
            }
            else if (User.Identity.Name != Username)
            {
                ViewBag.ErrorCode = 1;
                ViewBag.ErrorMsg = "أنت غير مفوض لدخول هذه الصفحة.";
                return View("Login");
            }
            UserProfileVM userProfile = new UserProfileVM();
            CompetitionRepository _compRepo = new CompetitionRepository();

            RWSUser user = _userRepo.GetUserByUsername(Username);
            userProfile.UserId = user.UserId;
            userProfile.UserName = user.UserName;
            userProfile.FirstName = user.FirstName;
            userProfile.LastName = user.LastName;
            //userProfile.articlesCount = user.Posts.Where(i => i.IsActive == true).Count();
            userProfile.profileImgUrl = user.ProfileImagePath;
            userProfile.DisplayName = user.DisplayName;
            userProfile.Google = user.Google;
            userProfile.Twitter = user.Twitter;
            userProfile.AboutYou = user.AboutYou;
            if (user.FavTeam != null && user.FavTeam.Value != 0)
            {
                userProfile.FavTeamId = user.FavTeam.Value;
                userProfile.FavTeamName = _compRepo.GetTeamById(user.FavTeam.Value).TeamName;
            }
            if (user.FavComp != null && user.FavComp.Value != 0)
            {
                userProfile.FavCompId = user.FavComp.Value;
                userProfile.FavCompName = _compRepo.GetCompetetionById(user.FavComp.Value).CompetitionName;
            }

            //ArticleRepository _articleRepo = new ArticleRepository();
            //userProfile.recentArticles = _articleRepo.GetRecentArticlesByUserId(user.UserId).ToList();
            //userProfile.viewsCount = _articleRepo.GetViewsCountByUserId(user.UserId);

            ViewBag.userProfile = userProfile;
            ViewBag.loggedInUser = User.Identity.Name;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] 
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        [HttpPost]
        public ActionResult UpdateProfileImg(string UserName, HttpPostedFileBase localPath)
        {
            // Verify that the user selected a file
            if (localPath != null && localPath.ContentLength > 0)
            {
                // extract only the fielname
                var fileName = Path.GetFileName(localPath.FileName);
                // store the file inside ~/App_Data/uploads folder
                var path = Path.Combine(Server.MapPath("/Content/Profile_Pictures"), fileName);
                localPath.SaveAs(path);
                RWSUser user = _userRepo.GetUserByUsername(UserName);
                _userRepo.UpdateProfileImg(user, "/Content/Profile_Pictures/" + fileName);
            }
            
            return Redirect("/Settings?Username=" + UserName);
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            ViewBag.Countries = GetAllCountries();
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (_userRepo.ExternalUserExists(result.ProviderUserId, result.Provider))
            {
                RWSUser user = _userRepo.GetExternalUser(result.ProviderUserId, result.Provider);
                FormsService.SignIn(user.UserName, true);
                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            if (result.ExtraData.Keys.Contains("accesstoken"))
            {
                Session["facebooktoken"] = result.ExtraData["accesstoken"];
            }

            if (User.Identity.IsAuthenticated)
            {
                _userRepo.CreateOrUpdateExternalUser(result.Provider, result.ProviderUserId);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;

                RegisterExternalLoginModel registrationData = new RegisterExternalLoginModel {
                    ExternalLoginData = loginData,
                    FullName = result.ExtraData["name"]
                };

                string provider = null;
                string providerUserId = null;

                if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(registrationData.ExternalLoginData, out provider, out providerUserId))
                {
                    return RedirectToAction("Manage");
                }

                if (ModelState.IsValid)
                {
                    if (!_userRepo.IsValidEmail(registrationData.Email))
                    {
                        ViewBag.provider = provider;
                        ViewBag.providerUserId = providerUserId;
                        var client = new Facebook.FacebookClient(Session["facebooktoken"].ToString());
                        dynamic response = client.Get("me", new
                        {
                            fields = "first_name, last_name, email"
                        });
                        dynamic picResponse = client.Get("me/picture", new { access_token = result.ExtraData["accesstoken"], redirect = false, height = 180, width = 180 });
                        string remoteImgPath = picResponse.data["url"];
                        Uri remoteImgPathUri = new Uri(remoteImgPath);
                        string remoteImgPathWithoutQuery = remoteImgPathUri.GetLeftPart(UriPartial.Path);
                        //string fileName = Path.GetFileName(remoteImgPathWithoutQuery);
                        string extention = Path.GetExtension(remoteImgPathWithoutQuery);
                        string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                        string fileName = time + "_" + response["first_name"] + "_" + response["last_name"]+extention;
                        string localPath = AppDomain.CurrentDomain.BaseDirectory + "Content\\Profile_Pictures\\" + fileName;
                        //string localPath = "Content\\Profile_Pictures\\" + fileName;
                        WebClient webClient = new WebClient();
                        webClient.DownloadFile(remoteImgPath, localPath);
                        registrationData.FirstName = response["first_name"];
                        registrationData.LastName = response["last_name"];
                        registrationData.Email = response["email"];
                        //registrationData.PicturePath = fileName;
                        registrationData.Link = "https://www.facebook.com/" + response["first_name"] + "/" + response["last_name"];
                        registrationData.PicturePath = "Content/Profile_Pictures/" + fileName;

                        if (_userRepo.CheckIfEmailExists(registrationData.Email))
                        {
                            RWSUser user = _userRepo.GetUserByEmail(registrationData.Email);
                            if (user.ProfileImagePath == null)
                            {
                                _userRepo.UpdateProfileImg(user, localPath);
                            }
                            RWSProviderUser provider_user = new RWSProviderUser();
                            provider_user.UserId = user.UserId;
                            provider_user.Provider = provider;
                            provider_user.ProviderUserId = providerUserId;
                            _userRepo.AddProviderUser(provider_user);

                            user = _userRepo.GetExternalUser(provider_user.ProviderUserId, provider_user.Provider);
                            FormsService.SignIn(user.UserName, true);
                            if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                                && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                            {
                                return Redirect(returnUrl);
                            }
                            return RedirectToAction("Index", "Home");
                        } 
                        else if (_userRepo.CheckIfEmailExists(registrationData.Email))
                        {
                            return View("Register2", registrationData);
                        }
                    }
                    else
                    {
                        //string email = registrationData.Email;
                        //RWSUser user = _userRepo.GetUserByEmail(email);
                        //LoginModel user_login = new LoginModel();
                        //user_login.UserName = user.UserName;
                        //user_login.Password = user.Password;
                        //Login(user_login, "");
                        //ModelState.AddModelError("UserName", "إسم المستخدم موجود بالفعل. يرجى إختيار إسم أخر.");
                    }
                }
                return View("Register2", registrationData);
            }
        }

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        [AllowAnonymous]
        public ActionResult UserPosts(string posts, string tags, int Page, string Username, int count = 8)
        {
            IndexVM result = GetFilteredArticles(posts, tags, Page, Username, count);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PostPartial", result);
            }
            return View(result);
        }

        [Authorize]
        public ActionResult Invitation(string Username)
        {
            @ViewBag.username = Username;
            return View();
        }

        [Authorize]
        public ActionResult SendInvitation(string friendEmail)
        {
            try
            {
                string userEmail = _userRepo.GetUserByUsername(User.Identity.Name).Email;
                dynamic email = new Email("InvitationEmail");
                email.To = friendEmail;
                email.UserEmail = userEmail;
                email.BaseUrl = Request.Url.Authority;
                email.Send();
                ViewBag.Message = "تم دعوة صديقك.";
                return View("ConfirmationEmailSent");
            }
            catch (Exception ex)
            {
                return View("ConfirmationFailure");
            }
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
