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
using System.Globalization;

namespace RayaWSoffara.Controllers
{
    [AllowAnonymous]
    public class HomeController : SearchController
    {
        ArticleRepository _articleRepo = new ArticleRepository();
        UserRepository _userRepo = new UserRepository();
        TutorialRepository _tutRepo = new TutorialRepository();

        [AllowAnonymous]
        public ActionResult Index(string posts, string tags, int? Page, string Username, string count)
        {
            IndexVM result = new IndexVM();

            if (count != null)
            {
                int displayedPostsCount = Int32.Parse(count);
                result = GetFilteredArticles(posts, tags, displayedPostsCount, null);
            }
            else if (posts != null || tags != null)
            {
                result = GetFilteredArticles(posts, tags, 0, null);
            }
            else
            {
                result = GetFilteredArticles(null, null, 0, null);
            }
            
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PostPartial", result);
            }

            ViewBag.AllActiveUsers = _userRepo.GetAllActiveUsers(null, null).Count();

            List<int> LeaderIdsAllTime = GetLeaderboardAuthorIds(null, null);
            List<UserPointsVM> AllTimeLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsAllTime)
            {
                AllTimeLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, null, null) });
            }
            ViewBag.AllTimeLeadersPoints = AllTimeLeadersPoints;

            //int weekId = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.UtcNow.ToLocalTime(), CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            DateTime WeekStartDate = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            DateTime WeekEndDate = WeekStartDate.AddDays(6);
            List<int> LeaderIdsWeekly = GetLeaderboardAuthorIds(WeekStartDate, WeekEndDate);
            List<UserPointsVM> WeeklyLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsWeekly)
            {
                WeeklyLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, WeekStartDate, WeekEndDate) });
            }
            ViewBag.WeeklyLeadersPoints = WeeklyLeadersPoints;

            DateTime MonthStartDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, 1);
            DateTime MonthEndDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, DateTime.DaysInMonth(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month));
            List<int> LeaderIdsMonthly = GetLeaderboardAuthorIds(MonthStartDate, MonthEndDate);
            List<UserPointsVM> MonthlyLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsMonthly)
            {
                MonthlyLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, MonthStartDate, MonthEndDate) });
            }
            ViewBag.MonthlyLeadersPoints = MonthlyLeadersPoints;

            if (User.Identity.Name != "")
            {
                int userId = _userRepo.GetUserByUsername(User.Identity.Name).UserId;
                UserTutorial userTut = _tutRepo.GetUserTutorialByTutAndUserId(1, userId);
                if (userTut == null || userTut.isViewed != true)
                {
                    ViewBag.tutorial = _tutRepo.GetTutorialById(1).TutScript;
                }
                else
                {
                    ViewBag.tutorial = "";
                }
            }

            return View(result);
        }

        public ActionResult ViewTutorial(int TutorialId, string UserName)
        {
            int userId = _userRepo.GetUserByUsername(UserName).UserId;
            _tutRepo.ViewTutorial(TutorialId, userId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Popular(string count)
        {
            IndexVM result = new IndexVM();

            if (count != null)
            {
                int displayedPostsCount = Int32.Parse(count);
                result = GetPopularPosts(displayedPostsCount);
            }
            else
            {
                result = GetPopularPosts(0);
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_PostPartial", result);
            }

            List<int> LeaderIdsAllTime = GetLeaderboardAuthorIds(null, null);
            List<UserPointsVM> AllTimeLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsAllTime)
            {
                AllTimeLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, null, null) });
            }
            ViewBag.AllTimeLeadersPoints = AllTimeLeadersPoints;

            //int weekId = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.UtcNow.ToLocalTime(), CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            DateTime WeekStartDate = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            DateTime WeekEndDate = WeekStartDate.AddDays(6);
            List<int> LeaderIdsWeekly = GetLeaderboardAuthorIds(WeekStartDate, WeekEndDate);
            List<UserPointsVM> WeeklyLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsWeekly)
            {
                WeeklyLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, WeekStartDate, WeekEndDate) });
            }
            ViewBag.WeeklyLeadersPoints = WeeklyLeadersPoints;

            DateTime MonthStartDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, 1);
            DateTime MonthEndDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, DateTime.DaysInMonth(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month));
            List<int> LeaderIdsMonthly = GetLeaderboardAuthorIds(MonthStartDate, MonthEndDate);
            List<UserPointsVM> MonthlyLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsMonthly)
            {
                MonthlyLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, MonthStartDate, MonthEndDate) });
            }
            ViewBag.MonthlyLeadersPoints = MonthlyLeadersPoints;

            return View("Index", result);
        }

        public ActionResult Leaderboard(DateTime? startDate, DateTime? endDate)
        {
            List<int> LeaderIdsAllTime = GetLeaderboardAuthorIds(null, null);
            List<UserPointsVM> AllTimeLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsAllTime)
            {
                AllTimeLeadersPoints.Add(new UserPointsVM { UserId = item, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, startDate, endDate) });
            }
            ViewBag.AllTimeLeadersPoints = AllTimeLeadersPoints;

            //int weekId = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.UtcNow.ToLocalTime(), CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            DateTime WeekStartDate = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            DateTime WeekEndDate = WeekStartDate.AddDays(7);
            List<int> LeaderIdsWeekly = GetLeaderboardAuthorIds(WeekStartDate, WeekEndDate);
            List<UserPointsVM> WeeklyLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsWeekly)
            {
                WeeklyLeadersPoints.Add(new UserPointsVM { UserId = item, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, WeekStartDate, WeekEndDate) });
            }
            ViewBag.WeeklyLeadersPoints = WeeklyLeadersPoints;

            DateTime MonthStartDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, 1);
            DateTime MonthEndDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, DateTime.DaysInMonth(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month));
            List<int> LeaderIdsMonthly = GetLeaderboardAuthorIds(MonthStartDate, MonthEndDate);
            List<UserPointsVM> MonthlyLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsMonthly)
            {
                MonthlyLeadersPoints.Add(new UserPointsVM { UserId = item, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, MonthStartDate, MonthEndDate) });
            }
            ViewBag.MonthlyLeadersPoints = MonthlyLeadersPoints;

            List<Post> posts = GetLeaderboardPosts(MonthStartDate, MonthEndDate);
            ViewBag.LeaderPosts = posts;
            
            return View();
        }

        public ActionResult Terms()
        {
            return View();
        }

        public ActionResult Privacy()
        {
            return View();
        }

        public ActionResult Advertising()
        {
            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Suggestions()
        {
            return View();
        }

        public ActionResult AddAdvertisement(string Email, string Text)
        {
            Advertisement adv = new Advertisement();
            adv.AdvertisementUserEmail = Email;
            adv.AdvertisementText = Text;
            adv.AdvertisementTimestamp = DateTime.UtcNow.ToLocalTime();
            _tutRepo.AddAdvertisement(adv);
            return Redirect("/ConfirmationSuccess");
        }

        public ActionResult AddSuggestion(string Email, string Text)
        {
            Suggestion suggestion = new Suggestion();
            suggestion.SuggestionUserEmail = Email;
            suggestion.SuggestionText = Text;
            suggestion.SuggestionTimestamp = DateTime.UtcNow.ToLocalTime();
            _tutRepo.AddSuggestion(suggestion);
            return Redirect("/ConfirmationSuccess");
        }
    }
}
