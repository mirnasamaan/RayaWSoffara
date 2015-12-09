﻿using System;
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
        public ActionResult Index(string posts, string tags, string count)
        {
            IndexVM result = new IndexVM();
            ArticleRepository _articleRepo = new ArticleRepository();
            UserRepository _userRepo = new UserRepository();

            if (count != null)
            {
                int displayedPostsCount = Int32.Parse(count);
                result = GetFilteredArticles(posts, tags, displayedPostsCount);
            }
            else if (posts != null || tags != null)
            {
                result = GetFilteredArticles(posts, tags, 0);
            }
            else
            {
                result = GetFilteredArticles(null, null, 0);
            }
            
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PostPartial", result);
            }

            ViewBag.AllActiveUsers = _userRepo.GetAllActiveUsers().Count();

            List<int> LeaderIdsAllTime = GetLeaderboardAuthorIds(null, null);
            List<UserPointsVM> AllTimeLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsAllTime)
            {
                AllTimeLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, null, null) });
            }
            ViewBag.AllTimeLeadersPoints = AllTimeLeadersPoints;

            //int weekId = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            DateTime WeekStartDate = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            DateTime WeekEndDate = WeekStartDate.AddDays(6);
            List<int> LeaderIdsWeekly = GetLeaderboardAuthorIds(WeekStartDate, WeekEndDate);
            List<UserPointsVM> WeeklyLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsWeekly)
            {
                WeeklyLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, WeekStartDate, WeekEndDate) });
            }
            ViewBag.WeeklyLeadersPoints = WeeklyLeadersPoints;

            DateTime MonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime MonthEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
            List<int> LeaderIdsMonthly = GetLeaderboardAuthorIds(MonthStartDate, MonthEndDate);
            List<UserPointsVM> MonthlyLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsMonthly)
            {
                MonthlyLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, MonthStartDate, MonthEndDate) });
            }
            ViewBag.MonthlyLeadersPoints = MonthlyLeadersPoints;

            return View(result);
        }

        public ActionResult Popular(string count)
        {
            IndexVM result = new IndexVM();
            ArticleRepository _articleRepo = new ArticleRepository();

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

            return View("Index", result);
        }

        public ActionResult Leaderboard(DateTime? startDate, DateTime? endDate)
        {
            ArticleRepository _articleRepo = new ArticleRepository();
            UserRepository _userRepo = new UserRepository();

            List<int> LeaderIdsAllTime = GetLeaderboardAuthorIds(null, null);
            List<UserPointsVM> AllTimeLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsAllTime)
            {
                AllTimeLeadersPoints.Add(new UserPointsVM { UserId = item, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, startDate, endDate) });
            }
            ViewBag.AllTimeLeadersPoints = AllTimeLeadersPoints;

            //int weekId = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            DateTime WeekStartDate = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            DateTime WeekEndDate = WeekStartDate.AddDays(7);
            List<int> LeaderIdsWeekly = GetLeaderboardAuthorIds(WeekStartDate, WeekEndDate);
            List<UserPointsVM> WeeklyLeadersPoints = new List<UserPointsVM>();
            foreach (var item in LeaderIdsWeekly)
            {
                WeeklyLeadersPoints.Add(new UserPointsVM { UserId = item, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, WeekStartDate, WeekEndDate) });
            }
            ViewBag.WeeklyLeadersPoints = WeeklyLeadersPoints;

            DateTime MonthStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime MonthEndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
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

    }
}
