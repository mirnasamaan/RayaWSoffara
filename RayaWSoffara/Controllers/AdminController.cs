﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RWSDataLayer.Context;
using RWSDataLayer.Repositories;
using RayaWSoffara.Models;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using System.Diagnostics;
using System.IO;
using ClosedXML.Excel;
using System.Data;
using RayaWSoffara.Helpers;
using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace RayaWSoffara.Controllers
{
    public class AdminController : Controller
    {
        UserRepository _userRepo = new UserRepository();
        CompetitionRepository _compRepo = new CompetitionRepository();
        ArticleRepository _postRepo = new ArticleRepository();
        EngagementRepository _engRepo = new EngagementRepository();
        TutorialRepository _tutRepo = new TutorialRepository();

        [AllowAnonymous]
        public ActionResult Signin(string RedirectUrl)
        {
            ViewBag.RedirectUrl = RedirectUrl;
            return View("Signin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] 
        public ActionResult Signin(string username, string password, string RedirectUrl)
        {
            RWSUser user = _userRepo.GetUserByUsernameAndPassword(username, password);
            bool userValid = _userRepo.UserExists(username, password);
            if (userValid)
            {
                password = GetMd5Hash(password);
                //var result = FormsAuthentication.Authenticate(username, password);
                FormsAuthentication.SetAuthCookie(username, true);
                return Redirect("/Admin/Dashboard");
            }
            else
            {
                ModelState.AddModelError("", "The user name or password provided is incorrect.");
            }
            return View();
        }

        public class Item
        {
            public string ItemName { get; set; }
            public int ItemId { get; set; }
        }

        public class DataTableData
        {
            public int draw { get; set; }
            public int recordsTotal { get; set; }
            public int recordsFiltered { get; set; }
            public List<DataItem> data { get; set; }
        }

        public class DataItem
        {
            public string ItemName { get; set; }
            public double articlesCount { get; set; }
            public string Points { get; set; }
            public string ActivePosts { get; set; }
            public string InactivePosts { get; set; }
            public string DT_RowId { get; set; }
            public string Status { get; set; }
            public string Actions { get; set; }
            public string Featured { get; set; }
            public string UserDelete { get; set; }
            public string CommentsCount { get; set; }
            public string ReportedCommentsCount { get; set; }
            public string isOriginal { get; set; }
            public string CreationDate { get; set; }
            public string ActivationDate { get; set; }
            public string Email { get; set; }
            public string ViewsCount { get; set; }
            public string LikesCount { get; set; }
            public string SharesCount { get; set; }
            public string ViewsValue { get; set; }
            public string LikesValue { get; set; }
            public string SharesValue { get; set; }
            public string Description { get; set; }
            public string Media { get; set; }
            public string Tags { get; set; }
            public string PostContent { get; set; }
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

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Signout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Signin", "Admin");
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetUserImage(string username)
        {
            string imgPath = _userRepo.GetUserByUsername(username).ProfileImagePath;
            return Json(imgPath, JsonRequestBehavior.AllowGet);
        }

        #region Dashboard
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Dashboard()
        {
            ViewBag.SidebarItem = "dashboard";
            ViewBag.PageHeader = "Dashboard";
            ViewBag.SubSidebarItem = "dashboard";
            Dashboard dashboard = new Dashboard();
            DateTime firstDayOfMonth = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, 1);
            DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            dashboard.AllTimeRegisteredUsers = _userRepo.GetUsersCount(null, null, null, null);
            dashboard.AllTimeActiveUsers = _userRepo.GetUsersCount("Active", null, null, null);
            dashboard.AllTimePendingUsers = _userRepo.GetUsersCount("Inactive", null, null, null);
            dashboard.AllTimePosts = _postRepo.GetPostsCount("", null, null, null, null);
            dashboard.AllTimeActivePosts = _postRepo.GetPostsCount("&Active", null, null, null, null);
            dashboard.AllTimePendingPosts = _postRepo.GetPostsCount("&Inactive", null, null, null, null);
            dashboard.AllTimeComments = _postRepo.GetAllCommentsCount(null, null, null, null, null);
            dashboard.AllTimeNonReportedComments = _postRepo.GetAllCommentsCount(null, null, "Nonreported", null, null);
            dashboard.AllTimeReportedComments = _postRepo.GetAllCommentsCount(null, null, "Reported", null, null);

            dashboard.RegisterdUsersThisMonth = _userRepo.GetUsersByCreationDate(DateTime.UtcNow.ToLocalTime().Month, DateTime.UtcNow.ToLocalTime().Year).Count();
            dashboard.ActivatedUsersThisMonth = _userRepo.GetUsersByActivationDate(DateTime.UtcNow.ToLocalTime().Month, DateTime.UtcNow.ToLocalTime().Year).Count();
            dashboard.PendingUsersThisMonth = _userRepo.GetPendingUsersByMonth(DateTime.UtcNow.ToLocalTime().Month, DateTime.UtcNow.ToLocalTime().Year).Count();
            dashboard.RegisteredUsersToday = _userRepo.GetUsersByCreationDate(DateTime.UtcNow.ToLocalTime()).Count();
            dashboard.ActivatedUsersToday = _userRepo.GetUsersByActivationDate(DateTime.UtcNow.ToLocalTime()).Count();
            dashboard.PendingUsersToday = _userRepo.GetPendingUsersByDate(DateTime.UtcNow.ToLocalTime()).Count();

            dashboard.TotalPostsThisMonth = _postRepo.GetPostsCount("", null, firstDayOfMonth, lastDayOfMonth, "");
            dashboard.ActivatedPostsThisMonth = _postRepo.GetPostsCount("Active", null, firstDayOfMonth, lastDayOfMonth, "");
            dashboard.PendingPostsThisMonth = _postRepo.GetPostsCount("Inactive", null, firstDayOfMonth, lastDayOfMonth, "");
            dashboard.TotalPostsToday = _postRepo.GetPostsCount("", null, DateTime.UtcNow.ToLocalTime(), DateTime.UtcNow.ToLocalTime(), "");
            dashboard.ActivatedPostsToday = _postRepo.GetPostsCount("Active", null, DateTime.UtcNow.ToLocalTime(), DateTime.UtcNow.ToLocalTime(), "");
            dashboard.PendingPostsThisMonth = _postRepo.GetPostsCount("Inactive", null, DateTime.UtcNow.ToLocalTime(), DateTime.UtcNow.ToLocalTime(), "");

            dashboard.TotalCommentsThisMonth = _postRepo.GetAllCommentsCount(null, null, "", firstDayOfMonth, lastDayOfMonth);
            dashboard.NonReportedCommentsThisMonth = _postRepo.GetAllCommentsCount(null, null, "NonReported", firstDayOfMonth, lastDayOfMonth);
            dashboard.ReportedCommentsThisMonth = _postRepo.GetAllCommentsCount(null, null, "Reported", firstDayOfMonth, lastDayOfMonth);
            dashboard.TotalCommentsToday = _postRepo.GetAllCommentsCount(null, null, "", DateTime.UtcNow.ToLocalTime(), DateTime.UtcNow.ToLocalTime());
            dashboard.NonreportedCommentsToday = _postRepo.GetAllCommentsCount(null, null, "Nonreported", DateTime.UtcNow.ToLocalTime(), DateTime.UtcNow.ToLocalTime());
            dashboard.ReportedCommentsToday = _postRepo.GetAllCommentsCount(null, null, "Reported", DateTime.UtcNow.ToLocalTime(), DateTime.UtcNow.ToLocalTime());
            return View(dashboard);
        }

        [CustomAuthorize(Roles = "Admin")]
        public static long GetJavascriptTimestamp(DateTime input)
        {
            TimeSpan span = new System.TimeSpan(DateTime.Parse("1/1/1970").Ticks);
            DateTime time = input.Subtract(span);
            return (long)(time.Ticks / 10000);

        }
      
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetUsersGraphPoints()
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

            double[][] active = new double[6][];
            for (int i = 0; i < 6; i++)
            {
                int month = beginMonth + i;
                int year = beginYear;
                if (month > 12)
                {
                    month -= 12;
                    year = beginYear + 1;
                }

                active[i] = new double[2];
                active[i][0] = GetJavascriptTimestamp(new DateTime(year, month, 1));
                active[i][1] = _userRepo.GetUsersByActivationDate(month, year).Count();
                //active[i][1] = _userRepo.GetAllActiveUsers(null, new DateTime(year, month, 1).AddMonths(1).AddDays(-1)).Count();
            }

            return Json(active, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetPostsGraphPoints()
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

            double[][] active = new double[6][];
            for (int i = 0; i < 6; i++)
            {
                int month = beginMonth + i;
                int year = beginYear;
                if (month > 12)
                {
                    month -= 12;
                    year = beginYear + 1;
                }
                DateTime fromDate = new DateTime(year, month, 1);
                DateTime toDate = fromDate.AddMonths(1).AddDays(-1);
                active[i] = new double[2];
                active[i][0] = GetJavascriptTimestamp(new DateTime(year, month, 1));
                active[i][1] = _postRepo.GetPosts(null, null, "Active", fromDate, toDate, null).Count();
                //active[i][1] = _userRepo.GetAllActiveUsers(null, new DateTime(year, month, 1).AddMonths(1).AddDays(-1)).Count();
            }

            return Json(active, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetCommentsGraphPoints()
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

            double[][] active = new double[6][];
            for (int i = 0; i < 6; i++)
            {
                int month = beginMonth + i;
                int year = beginYear;
                if (month > 12)
                {
                    month -= 12;
                    year = beginYear + 1;
                }
                DateTime fromDate = new DateTime(year, month, 1);
                DateTime toDate = fromDate.AddMonths(1).AddDays(-1);
                active[i] = new double[2];
                active[i][0] = GetJavascriptTimestamp(new DateTime(year, month, 1));
                active[i][1] = _postRepo.GetComments(null, null, null, "Nonreported", fromDate, toDate, null).Count();
                //active[i][1] = _userRepo.GetAllActiveUsers(null, new DateTime(year, month, 1).AddMonths(1).AddDays(-1)).Count();
            }

            return Json(active, JsonRequestBehavior.AllowGet);
        }

        public class GraphData
        {
            public string label { get; set; }
            public int value { get; set; }
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetTopTags()
        {
            IQueryable<Tag> topTags = _postRepo.GetTopTags(5);

            List<GraphData> dataList = new List<GraphData>();
            foreach (var item in topTags)
            {
                GraphData details = new GraphData();
                details.label = _postRepo.GetTagById(item.TagId).TagName;
                details.value = _postRepo.GetPostsWithTagIDs(new List<int> { item.TagId }, null).Count();

                dataList.Add(details);
            }
            return Json(dataList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Users
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Users()
        {
            ViewBag.SidebarItem = "users-management";
            ViewBag.PageHeader = "Users Management";
            ViewBag.SubSidebarItem = "users";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetUsers(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            int total_rows = _userRepo.GetUsersCount(null, null, null, null);
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            int displayedNum;
            List<DataItem> userprofiles = new List<DataItem>();
            int pageNum = start / length;
            IQueryable<RWSUser> users = null;
            if (search == "")
            {
                users = _userRepo.GetAllUsers(pageNum, length, "", null, null, out displayedNum);
            }
            else
            {
                users = _userRepo.GetUsersBySearchTerm(start, length, "", search, null, null);
            }
            foreach (var item in users)
            {
                string status;
                if (item.IsConfirmed == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }

                userprofiles.Add(new DataItem { ItemName = item.UserName, articlesCount = _userRepo.GetUserPostsCount(item.UserId, "Active"), Actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>", DT_RowId = item.UserName, Status = status });
            }
            dataTableData.data = userprofiles;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteUser(string Username)
        {
            RWSUser user = _userRepo.GetUserByUsername(Username);
            _userRepo.DeleteUser(user);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddUser()
        {
            List<RWSRole> roles = _userRepo.GetRoles().ToList();
            ViewBag.Roles = roles;
            ViewBag.SidebarItem = "users-management";
            ViewBag.PageHeader = "Users Management";
            ViewBag.SubSidebarItem = "users";
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddUser(RWSUser user, string roleName)
        {
            RWSRole role = _userRepo.GetRoleByName(roleName);
            user.RWSRoles.Clear();
            user.RWSRoles.Add(role);
            user.Password = GetMd5Hash(user.Password);
            user.IsConfirmed = true;
            user.ConfirmationDate = DateTime.UtcNow.ToLocalTime();
            _userRepo.AddUser(user);
            ViewBag.AddSuccess = true;
            return View("Users");
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditUser(string UserName)
        {
            RWSUser user = _userRepo.GetUserByUsername(UserName);
            List<RWSRole> roles = _userRepo.GetRoles().ToList();
            ViewBag.Roles = roles;
            ViewBag.SidebarItem = "users-management";
            ViewBag.PageHeader = "Users Management";
            ViewBag.SubSidebarItem = "users";
            return View(user);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditUser(RWSUser user, string roleName, int UserId)
        {
            RWSUser db_user = _userRepo.GetUserByUserId(UserId);
            db_user.UserName = user.UserName;
            db_user.Email = user.Email;
            RWSRole role = _userRepo.GetRoleByName(roleName);
            _userRepo.DeleteUserRoles(db_user.UserId);
            db_user.RWSRoles.Add(role);
            _userRepo.UpdateUserDetails(db_user);
            ViewBag.EditSuccess = true;
            return Redirect("/Admin/Users");
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeactivateUser(string username)
        {
            _userRepo.DeactivateUser(username);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ActivateUser(string username)
        {
            _userRepo.ActivateUser(username);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Regions
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Regions()
        {
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            ViewBag.SubSidebarItem = "regions";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteRegion(int RegionId)
        {
            Region region = _compRepo.GetRegionById(RegionId);
            Tag tag = _postRepo.GetTagByTagTypeAndTagTypeId(1, RegionId);
            _compRepo.DeleteRegion(region);
            _postRepo.DeleteTag(tag);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddRegion()
        {
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            ViewBag.SubSidebarItem = "regions";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddRegion(Region region, HttpPostedFileBase path)
        {
            region.IsActive = true;
            Region reg = _compRepo.AddRegion(region);
            Tag tag = new Tag();
            tag.TagName = region.RegionName;
            tag.TagType = 1;
            tag.TagTypeId = reg.RegionId;
            tag.isActive = true;
            _postRepo.AddTag(tag);
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + region.RegionName + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Region_Logos"), fileName);
                path.SaveAs(localpath);
                reg.RegionIcon = "/Content/Region_Logos/" + fileName;
                _compRepo.UpdateRegion(reg);
            }
            return Redirect("/Admin/Regions");
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetRegions(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            int total_rows = _compRepo.GetAllRegions().Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> regionprofiles = new List<DataItem>();
            IQueryable<Region> regions = null;
            if (search == "")
            {
                regions = _compRepo.GetAllRegions(start, length);
            }
            else
            {
                regions = _compRepo.GetRegionsBySearchTerm(start, length, search);
            }
            foreach (var item in regions)
            {
                string status;
                if (item.IsActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                regionprofiles.Add(new DataItem { ItemName = item.RegionName, Actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>", Status = status, DT_RowId = item.RegionId.ToString() });
            }
            dataTableData.data = regionprofiles;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditRegion(int RegionId)
        {
            Region region = _compRepo.GetRegionById(RegionId);
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            ViewBag.SubSidebarItem = "regions";
            return View(region);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditRegion(Region region, HttpPostedFileBase path, int RegionId)
        {
            Region db_region = _compRepo.GetRegionById(RegionId);
            Tag tag = _postRepo.GetTagByName(db_region.RegionName);
            db_region.RegionName = region.RegionName;
            tag.TagName = region.RegionName;
            if (path != null && path.ContentLength > 0)
            {
                // extract only the fielname
                //var fileName = Path.GetFileName(path.FileName);
                // store the file inside ~/App_Data/uploads folder
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + region.RegionName.Replace(' ', '_') + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Region_Logos"), fileName);
                path.SaveAs(localpath);
                db_region.RegionIcon = "/Content/Region_Logos/" + fileName;
            }
            _compRepo.UpdateRegion(db_region);
            _postRepo.UpdateTag(tag);
            return Redirect("/Admin/Regions");
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeactivateRegion(int RegionId)
        {
            _compRepo.DeactivateRegion(RegionId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ActivateRegion(int RegionId)
        {
            _compRepo.ActivateRegion(RegionId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Competitions
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Competitions()
        {
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            ViewBag.SubSidebarItem = "competitions";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteCompetition(int CompId)
        {
            Competition competition = _compRepo.GetCompetetionById(CompId);
            Tag tag = _postRepo.GetTagByTagTypeAndTagTypeId(2, CompId);
            _compRepo.DeleteCompetition(competition);
            _postRepo.DeleteTag(tag);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddCompetition()
        {
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            ViewBag.SubSidebarItem = "competitions";
            List<Region> regions = _compRepo.GetAllRegions().ToList();
            return View(regions);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddCompetition(Competition competition, HttpPostedFileBase path, string teams)
        {
            if (teams != "")
            {
                string[] teamIds = teams.Split(',');
                foreach (var item in teamIds)
                {
                    competition.Teams.Add(_compRepo.GetTeamById(Int32.Parse(item)));
                }
            }
            competition.isActive = true;
            Competition comp = _compRepo.AddCompetition(competition);
            comp.isActive = true;
            Tag tag = new Tag();
            tag.TagName = comp.CompetitionName;
            tag.TagType = 2;
            tag.isActive = true;
            tag.TagTypeId = comp.CompetitionId;
            _postRepo.AddTag(tag);
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + comp.CompetitionName + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Competition_Logos"), fileName);
                path.SaveAs(localpath);
                comp.CompetitionIcon = "/Content/Competition_Logos/" + fileName;
                _compRepo.UpdateCompetition(comp);
            }
            return Redirect("/Admin/Competitions");
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult GetTeamsByCompetitionId(int CompId)
        {
            IQueryable<Team> db_teams = _compRepo.GetTeamsForCompetition(CompId);
            List<Item> teams = new List<Item>();
            foreach (var item in db_teams)
            {
                teams.Add(new Item { ItemId = item.TeamId, ItemName = item.TeamName });
            }
            return Json(teams, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetCompetitions(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            var watch = Stopwatch.StartNew();
            int total_rows = _compRepo.GetAllCompetetions().Count();
            watch.Stop();
            var elapsedMs1 = watch.ElapsedMilliseconds;
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> competitionprofiles = new List<DataItem>();
            watch = Stopwatch.StartNew();
            IQueryable<Competition> competitions = null;
            if (search == "")
            {
                competitions = _compRepo.GetAllCompetetions(start, length);
            }
            else
            {
                competitions = _compRepo.GetCompetitionsBySearchTerm(start, length, search);
            }
            watch.Stop();
            var elapsedMS2 = watch.ElapsedMilliseconds;
            foreach (var item in competitions)
            {
                string status;
                if (item.isActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                competitionprofiles.Add(new DataItem { ItemName = item.CompetitionName, Actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>", Status = status, DT_RowId = item.CompetitionId.ToString() });
            }
            dataTableData.data = competitionprofiles;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditCompetition(int CompId)
        {
            Competition comp = _compRepo.GetCompetitionById(CompId);
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            ViewBag.SubSidebarItem = "competitions";
            ViewBag.Regions = _compRepo.GetAllRegions().ToList();
            return View(comp);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditCompetition(Competition comp, int RegionId, HttpPostedFileBase path, int CompId, string teams)
        {
            Competition db_comp = _compRepo.GetCompetetionById(CompId);
            Tag tag = _postRepo.GetTagByTagTypeAndTagTypeId(2, db_comp.CompetitionId);
            db_comp.CompetitionName = comp.CompetitionName;
            db_comp.RegionId = RegionId;
            _compRepo.DeleteAllTeamsForCompetition(CompId);
            if (teams != "")
            {
                string[] teamIds = teams.Split(',');
                foreach (var item in teamIds)
                {
                    db_comp.Teams.Add(_compRepo.GetTeamById(Int32.Parse(item)));
                }
            }
            tag.TagName = comp.CompetitionName;
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + comp.CompetitionName.Replace(' ', '_') + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Competition_Logos"), fileName);
                path.SaveAs(localpath);
                db_comp.CompetitionIcon = "/Content/Competition_Logos/" + fileName;
            }
            _compRepo.UpdateCompetition(db_comp);
            _postRepo.UpdateTag(tag);
            return Redirect("/Admin/Competitions");
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeactivateCompetition(int CompId)
        {
            _compRepo.DeactivateCompetition(CompId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ActivateCompetition(int CompId)
        {
            _compRepo.ActivateCompetition(CompId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetCompetitions()
        {
            List<Item> comps = new List<Item>();
            IQueryable<Competition> db_comps = _compRepo.GetAllCompetetions();
            foreach (var item in db_comps)
            {
                comps.Add(new Item() { ItemId = item.CompetitionId, ItemName = item.CompetitionName });
            }
            return Json(comps, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Teams
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Teams()
        {
            ViewBag.SubSidebarItem = "teams";
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteTeam(int TeamId)
        {
            Team team = _compRepo.GetTeamById(TeamId);
            Tag tag = _postRepo.GetTagByTagTypeAndTagTypeId(3, TeamId);
            _compRepo.DeleteTeam(team);
            _postRepo.DeleteTag(tag);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddTeam()
        {
            ViewBag.SubSidebarItem = "teams";
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddTeam(Team team, HttpPostedFileBase path, string competitions, string players)
        {
            List<int> compIds = null;
            if (competitions != "")
            {
                compIds = competitions.Split(',').Select(s => int.Parse(s)).ToList();
            }
            team.isActive = true;
            Team db_team = _compRepo.AddTeam(team, compIds);
            if (players != "")
            {
                List<int> playerIds = players.Split(',').Select(s => int.Parse(s)).ToList();
                _compRepo.AddTeamPlayers(db_team, playerIds);
            }
            Tag tag = new Tag();
            tag.TagName = db_team.TeamName;
            tag.TagType = 3;
            tag.isActive = true;
            tag.TagTypeId = db_team.TeamId;
            _postRepo.AddTag(tag);
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + db_team.TeamName + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Team_Logos"), fileName);
                path.SaveAs(localpath);
                db_team.TeamIcon = "/Content/Team_Logos/" + fileName;
                _compRepo.UpdateTeam(db_team, compIds);
            }
            return Redirect("/Admin/Teams");
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetTeams(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            var watch = Stopwatch.StartNew();
            int total_rows = _compRepo.GetAllTeams().Count();
            watch.Stop();
            var elapsedMs1 = watch.ElapsedMilliseconds;
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> teamprofiles = new List<DataItem>();
            watch = Stopwatch.StartNew();
            IQueryable<Team> teams = null;
            if (search == "")
            {
                teams = _compRepo.GetAllTeams(start, length);
            }
            else
            {
                teams = _compRepo.GetTeamsBySearchTerm(start, length, search);
            }
            watch.Stop();
            var elapsedMS2 = watch.ElapsedMilliseconds;
            foreach (var item in teams)
            {
                string status;
                if (item.isActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                teamprofiles.Add(new DataItem { ItemName = item.TeamName, Actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>", Status = status, DT_RowId = item.TeamId.ToString() });
            }
            dataTableData.data = teamprofiles;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult GetCompetitionsByTeamId(int TeamId)
        {
            IQueryable<Competition> db_comps = _compRepo.GetCompetitionsForTeam(TeamId);
            List<Item> comps = new List<Item>();
            foreach (var item in db_comps)
            {
                comps.Add(new Item { ItemId = item.CompetitionId, ItemName = item.CompetitionName });
            }
            return Json(comps, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult GetPlayersByTeamId(int TeamId)
        {
            IQueryable<Player> db_players = _compRepo.GetPlayersForTeam(TeamId);
            List<Item> players = new List<Item>();
            foreach (var item in db_players)
            {
                players.Add(new Item { ItemId = item.PlayerId, ItemName = item.PlayerName });
            }
            return Json(players, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditTeam(int TeamId)
        {
            Team team = _compRepo.GetTeamById(TeamId);
            ViewBag.SubSidebarItem = "teams";
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            return View(team);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditTeam(Team team, HttpPostedFileBase path, int TeamId, string competitions, string players)
        {
            Team db_team = _compRepo.GetTeamById(TeamId);
            _compRepo.DeleteAllCompetitionsForTeam(TeamId);
            _compRepo.DeleteAllPlayersForTeam(TeamId);
            List<int> compIds = null;
            if (competitions != "")
            {
                compIds = competitions.Split(',').Select(s => int.Parse(s)).ToList();
            }
            if (players != "")
            {
                List<int> playerIds = players.Split(',').Select(s => int.Parse(s)).ToList();
                _compRepo.AddTeamPlayers(db_team, playerIds);
            }
            Tag tag = _postRepo.GetTagByTagTypeAndTagTypeId(3, db_team.TeamId);
            db_team.TeamName = team.TeamName;
            tag.TagName = db_team.TeamName;
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + db_team.TeamName.Replace(' ', '_') + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Team_Logos"), fileName);
                path.SaveAs(localpath);
                db_team.TeamIcon = "/Content/Team_Logos/" + fileName;
            }
            _compRepo.UpdateTeam(db_team, compIds);
            _postRepo.UpdateTag(tag);
            return Redirect("/Admin/Teams");
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeactivateTeam(int TeamId)
        {
            _compRepo.DeactivateTeam(TeamId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ActivateTeam(int TeamId)
        {
            _compRepo.ActivateTeam(TeamId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetTeams()
        {
            List<Item> teams = new List<Item>();
            IQueryable<Team> db_teams = _compRepo.GetAllTeams();
            foreach (var item in db_teams)
            {
                teams.Add(new Item() { ItemId = item.TeamId, ItemName = item.TeamName });
            }
            return Json(teams, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Players
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Players()
        {
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            ViewBag.SubSidebarItem = "players";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeletePlayer(int PlayerId)
        {
            Player player = _compRepo.GetPlayerById(PlayerId);
            Tag tag = _postRepo.GetTagByTagTypeAndTagTypeId(4, PlayerId);
            _compRepo.DeletePlayer(player);
            _postRepo.DeleteTag(tag);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddPlayer()
        {
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            ViewBag.SubSidebarItem = "palyers";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddPlayer(Player player, HttpPostedFileBase path, string teams)
        {
            List<int> teamIds = null;
            if (teams != "")
            {
                teamIds = teams.Split(',').Select(s => int.Parse(s)).ToList();
            }
            player.isActive = true;
            Player db_player = _compRepo.AddPlayer(player, teamIds);

            Tag tag = new Tag();
            tag.TagName = db_player.PlayerName;
            tag.TagType = 4;
            tag.isActive = true;
            tag.TagTypeId = db_player.PlayerId;
            _postRepo.AddTag(tag);
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + db_player.PlayerName + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Player_Logos"), fileName);
                path.SaveAs(localpath);
                db_player.PlayerIcon = "/Content/Player_Logos/" + fileName;
                _compRepo.UpdatePlayer(db_player, teamIds);
            }
            return Redirect("/Admin/Players");
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetPlayers(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            int total_rows = _compRepo.GetAllPlayers().Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> playerplrofiles = new List<DataItem>();
            IQueryable<Player> players = null;
            if (search == "")
            {
                players = _compRepo.GetAllPlayers(start, length);
            }
            else
            {
                players = _compRepo.GetPlayersBySearchTerm(start, length, search);
            }
            foreach (var item in players)
            {
                string status;
                if (item.isActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }

                playerplrofiles.Add(new DataItem { ItemName = item.PlayerName, Actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>", Status = status, DT_RowId = item.PlayerId.ToString() });
            }
            dataTableData.data = playerplrofiles;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult GetTeamsByPlayerId(int PlayerId)
        {
            IQueryable<Team> db_teams = _compRepo.GetTeamsForPlayer(PlayerId);
            List<Item> teams = new List<Item>();
            foreach (var item in db_teams)
            {
                teams.Add(new Item { ItemId = item.TeamId, ItemName = item.TeamName });
            }
            return Json(teams, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditPlayer(int PlayerId)
        {
            Player player = _compRepo.GetPlayerById(PlayerId);
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            ViewBag.SubSidebarItem = "palyers";
            return View(player);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditPlayer(Player player, HttpPostedFileBase path, int PlayerId, string teams)
        {
            List<int> teamIds = null;
            if (teams != "")
            {
                teamIds = teams.Split(',').Select(s => int.Parse(s)).ToList();
            }
            Player db_player = _compRepo.GetPlayerById(PlayerId);
            Tag tag = _postRepo.GetTagByTagTypeAndTagTypeId(4, db_player.PlayerId);
            db_player.PlayerName = player.PlayerName;
            tag.TagName = player.PlayerName;
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + player.PlayerName.Replace(' ', '_') + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Player_Logos"), fileName);
                path.SaveAs(localpath);
                db_player.PlayerIcon = "/Content/Player_Logos/" + fileName;
            }
            _compRepo.DeleteAllTeamsForPlayer(PlayerId);
            _compRepo.UpdatePlayer(db_player, teamIds);
            _postRepo.UpdateTag(tag);
            return Redirect("/Admin/Players");
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeactivatePlayer(int PlayerId)
        {
            _compRepo.DeactivatePlayer(PlayerId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ActivatePlayer(int PlayerId)
        {
            _compRepo.ActivatePlayer(PlayerId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetPlayers()
        {
            List<Item> players = new List<Item>();
            IQueryable<Player> db_players = _compRepo.GetAllPlayers();
            foreach (var item in db_players)
            {
                players.Add(new Item() { ItemId = item.PlayerId, ItemName = item.PlayerName });
            }
            return Json(players, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Tags
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Tags()
        {
            ViewBag.SubSidebarItem = "tags";
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetTags(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            var watch = Stopwatch.StartNew();
            int total_rows = _postRepo.GetTags().Count();
            watch.Stop();
            var elapsedMs1 = watch.ElapsedMilliseconds;
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> tagprofiles = new List<DataItem>();
            watch = Stopwatch.StartNew();
            IQueryable<Tag> tags = null;
            if (search == "")
            {
                tags = _postRepo.GetAllTags(start, length);
            }
            else
            {
                tags = _postRepo.GetTagsBySearchTerm(start, length, search);
            }
            watch.Stop();
            var elapsedMS2 = watch.ElapsedMilliseconds;
            foreach (var item in tags)
            {
                string status;
                if (item.isActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                string actions;
                if (item.TagTypeId == null)
                {
                    actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";
                }
                else
                {
                    actions = "<a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";
                }
                string featured;
                if (item.isFeatured == true)
                {
                    featured = "<span onclick='RemoveFeatured(this)' class='status-action label label-warning'>Featured</span>";
                }
                else
                {
                    featured = "<span onclick='SetFeatured(this)' class='status-action label label-default'>Not Featured</span>";
                }
                tagprofiles.Add(new DataItem { ItemName = item.TagName, Actions = actions, Status = status, Featured = featured, DT_RowId = item.TagId.ToString() });
            }
            dataTableData.data = tagprofiles;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult RemoveTagAsFeatured(int TagId)
        {
            _postRepo.RemoveTagAsFeatured(TagId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult SetTagAsFeatured(int TagId)
        {
            _postRepo.SetTagAsFeatured(TagId);
            ActivateTag(TagId);
            Tag tag = _postRepo.GetTagById(TagId);
            switch (tag.TagType)
            {
                case 1:
                    _compRepo.ActivateRegion(tag.TagTypeId.Value);
                    break;
                case 2:
                    _compRepo.ActivateCompetition(tag.TagTypeId.Value);
                    break;
                case 3:
                    _compRepo.ActivateTeam(tag.TagTypeId.Value);
                    break;
                case 4:
                    _compRepo.ActivatePlayer(tag.TagTypeId.Value);
                    break;
                default: break;
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddTag()
        {
            ViewBag.SubSidebarItem = "tags";
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddTag(Tag tag, HttpPostedFileBase path)
        {
            tag.isActive = true;
            _postRepo.AddTag(tag);
            return Redirect("/Admin/Tags");
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteTag(int TagId)
        {
            Tag tag = _postRepo.GetTagById(TagId);
            _postRepo.DeleteTag(tag);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditTag(int TagId)
        {
            Tag tag = _postRepo.GetTagById(TagId);
            ViewBag.SubSidebarItem = "tags";
            ViewBag.SidebarItem = "tags-management";
            ViewBag.PageHeader = "Tags Management";
            return View(tag);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditTag(Tag tag, int TagId)
        {
            Tag db_tag = _postRepo.GetTagById(TagId);
            tag.TagName = tag.TagName;
            _postRepo.UpdateTag(tag);
            return Redirect("/Admin/Tags");
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeactivateTag(int TagId)
        {
            _postRepo.DeactivateTag(TagId);
            _postRepo.RemoveTagAsFeatured(TagId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ActivateTag(int TagId)
        {
            _postRepo.ActivateTag(TagId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Images
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Images(int? page)
        {
            ViewBag.SidebarItem = "images-management";
            ViewBag.PageHeader = "Images Management";
            ViewBag.SubSidebarItem = "images";
            if (page == null)
            {
                page = 1;
            }
            List<Image> images = _compRepo.GetImages(page.Value).ToList();
            ViewBag.AllImagesCount = _compRepo.GetAllImages().Count();
            if (Request.IsAjaxRequest())
            {
                return PartialView("_ImagesPartial", images);
            }
            else
            {
                return View(images);
            }
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteImage(int ImageId)
        {
            Image image = _compRepo.GetImageById(ImageId);
            string filePath = Server.MapPath("~" + image.ImageURL);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            _compRepo.DeleteImage(image);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddImages()
        {
            ViewBag.SidebarItem = "images-management";
            ViewBag.PageHeader = "Images Management";
            ViewBag.SubSidebarItem = "images";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AddImages(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                string extention = Path.GetExtension(file.FileName);
                string time = DateTime.UtcNow.ToLocalTime().ToString("yyyymmddhhmmssfff");
                string fileName = Path.GetFileNameWithoutExtension(file.FileName) + "_" + time + Path.GetExtension(file.FileName);
                var localpath = Path.Combine(Server.MapPath("/Content/Article_Images"), fileName);
                file.SaveAs(localpath);
                Image image = new Image();
                image.ImageURL = "/Content/Article_Images/" + fileName;
                image = _compRepo.AddImage(image);
                return Json(new { UniqueId = image.ImageId });
            }
            return Redirect("Admin/Images");
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult GetTagsByImageId(int ImageId)
        {
            IQueryable<Tag> db_tags = _compRepo.GetTagsForImage(ImageId);
            List<Item> tags = new List<Item>();
            foreach (var item in db_tags)
            {
                tags.Add(new Item { ItemId = item.TagId, ItemName = item.TagName });
            }
            return Json(tags, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddTagsToImage(int ImageId, string tagItems)
        {
            List<int> tagIds = null;
            if (tagItems != "")
            {
                tagIds = tagItems.Split(',').Select(s => int.Parse(s)).ToList();
            }
            Image image = _compRepo.GetImageById(ImageId);
            _compRepo.DeleteAllTagsForImage(ImageId);
            _compRepo.AddTagToImage(image, tagIds);
            return Redirect("/Admin/Images");
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetImageById(int ImageId)
        {
            Image image = _compRepo.GetImageById(ImageId);
            if (Request.IsAjaxRequest())
            {
                return PartialView("_UploadsPartial", image);
            }
            return null;
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditImage(int ImageId, string ImageName, string SelectedTags)
        {
            List<int> tagIds = null;
            if (SelectedTags != "")
            {
                tagIds = SelectedTags.Split(',').Select(s => int.Parse(s)).ToList();
            }

            Image img = _compRepo.GetImageById(ImageId);
            string oldNameFullPath = img.ImageURL;
            string path = img.ImageURL.Substring(0, img.ImageURL.LastIndexOf("/"));
            string uniqueNamePart = img.ImageURL.Substring(img.ImageURL.LastIndexOf("_"));
            img.ImageURL = path + "/" + ImageName + uniqueNamePart;
            System.IO.File.Move(AppDomain.CurrentDomain.BaseDirectory + oldNameFullPath, AppDomain.CurrentDomain.BaseDirectory + img.ImageURL);

            _compRepo.DeleteAllTagsForImage(img.ImageId);
            img = _compRepo.UpdateImage(img, tagIds);

            ImageDataItem data = new ImageDataItem();
            data.ImageName = img.ImageURL;
            data.Tags = new List<Item>();
            foreach(var item in img.Tags) {
                data.Tags.Add(new Item { ItemId = item.TagId, ItemName = item.TagName });
            }

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public class ImageDataItem
        {
            public string ImageName { set; get; }
            public List<Item> Tags { get; set; }
        }
        #endregion

        #region Articles
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Articles()
        {
            ViewBag.SubSidebarItem = "articles";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetArticles(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            int total_rows = _postRepo.GetPosts(1, null).Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> posts = new List<DataItem>();
            IQueryable<Post> posts_items = _postRepo.GetPosts(1, start, length);
            foreach (var item in posts_items)
            {
                string status, isOriginal;
                string userdelete = "";
                string commentsCount = "<span onclick='SeeComments(" + item.PostId + ")' class='comments-count'>" + item.Comments.Count() + "</span>";
                int reportedCommentsCount = item.Comments.Sum(i => i.CommentReportCount);
                if (item.IsActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                if (item.isOriginal == true)
                {
                    isOriginal = "<span onclick='UnsetOriginal(this)' class='status-action label label-success'>Original</span>";
                }
                else
                {
                    isOriginal = "<span onclick='SetOriginal(this)' class='status-action label label-danger'>Copied</span>";
                }
                if (item.isDeleted == true) {
                    userdelete = "<span class='status-action label label-danger'>Deleted</span>";
                }
                string actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";

                posts.Add(new DataItem { ItemName = item.Title, Actions = actions, Status = status, UserDelete = userdelete, CommentsCount = commentsCount, ReportedCommentsCount = reportedCommentsCount.ToString(), isOriginal = isOriginal, DT_RowId = item.PostId.ToString() });
            }
            dataTableData.data = posts;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddArticle()
        {
            ViewBag.SubSidebarItem = "articles";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult AddArticle(UserArticleVM article, string article_picture_path, string video_url)
        {
            //IEnumerable<Tag> articlesTags = _postRepo.GetTags();
            //ViewBag.tags = articlesTags.ToList();
            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            article.newArticle.CreatedBy = currentUser.UserId;
            article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.ActivationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.MetaTags = "";
            List<Tag> tags = _postRepo.getSelectedTags(article.SelectedTags).ToList();
            article.newArticle.Tags = tags;
            HttpPostedFileBase picture = Request.Files[0];
            if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                    if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "Temp/" };
                        string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.newArticle.FeaturedImage = imgName;
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    article.newArticle.FeaturedImage = picName;
                }

                article.newArticle.HasImage = true;
            }
            else if (video_url != null || video_url != string.Empty)
            {
                article.newArticle.HasImage = false;
                article.newArticle.FeaturedVideo = video_url;
            }

            article.newArticle.MetaTags = "";
            article.newArticle.ViewsCount = 0;
            article.newArticle.SharesCount = 0;
            article.newArticle.PostTypeId = 1;
            article.newArticle.IsActive = true;
            Post addedArticle = _postRepo.AddPost(article.newArticle);
            if (addedArticle != null)
            {
                ViewBag.ErrorMsg = 0;
                return RedirectToAction("Articles");
            }
            else
            {
                ViewBag.ErrorMsg = 1;
                return View();
            }
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetTagsForPost(int PostId)
        {
            IQueryable<Tag> db_tags = _postRepo.GetTagsForPost(PostId);
            List<Item> tags = new List<Item>();
            foreach (var item in db_tags)
            {
                tags.Add(new Item { ItemId = item.TagId, ItemName = item.TagName });
            }
            return Json(tags, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditArticle(int PostId)
        {
            Post article = _postRepo.GetPostById(PostId);
            UserArticleVM articleVM = new UserArticleVM();
            articleVM.newArticle = article;
            ViewBag.SubSidebarItem = "articles";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View(articleVM);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult EditArticle(UserArticleVM articleVM, string article_picture_path, string video_url)
        {
            Post article = _postRepo.GetPostById(articleVM.newArticle.PostId);
            article.Title = articleVM.newArticle.Title;
            article.Content = articleVM.newArticle.Content;
            List<Tag> tags = _postRepo.getSelectedTags(articleVM.SelectedTags).ToList();
            HttpPostedFileBase picture = Request.Files[0];
            if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    if (article_picture_path.Contains("Temp"))
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                        if (System.IO.File.Exists(path))
                        {
                            string[] separator = new string[] { "Temp/" };
                            string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                            string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                            System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                            article.FeaturedImage = imgName;
                        }
                    }
                    else
                    {
                        string[] imgName = article_picture_path.Split('/');
                        article.FeaturedImage = imgName.Last();
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    article.FeaturedImage = picName;
                }

                article.HasImage = true;
            }
            else if (video_url != null || video_url != string.Empty)
            {
                article.HasImage = false;
                article.FeaturedVideo = video_url;
            }
            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            _postRepo.UpdateArticle(article, currentUser);
            _postRepo.UpdatedArticleTags(articleVM.newArticle.PostId, tags);
            return Redirect("/Admin/Articles");
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult SetOriginal(int PostId)
        {
            _postRepo.SetOriginal(PostId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult UnsetOriginal(int PostId)
        {
            _postRepo.UnsetOriginal(PostId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeactivatePost(int PostId)
        {
            _postRepo.DeactivatePost(PostId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ActivatePost(int PostId)
        {
            _postRepo.ActivatePost(PostId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }


        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeletePost(int PostId)
        {
            Post post = _postRepo.GetPostById(PostId);
            _postRepo.DeletePost(post);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ArticlesTopX
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ArticlesTopX()
        {
            ViewBag.SubSidebarItem = "lists";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetArticlesTopX(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            int total_rows = _postRepo.GetPosts(2, null).Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> posts = new List<DataItem>();
            IQueryable<Post> posts_items = _postRepo.GetPosts(2, start, length);
            foreach (var item in posts_items)
            {
                string status, isOriginal;
                string userdelete = "";
                string commentsCount = "<span onclick='SeeComments(" + item.PostId + ")' class='comments-count'>" + item.Comments.Count() + "</span>";
                int reportedCommentsCount = item.Comments.Sum(i => i.CommentReportCount);
                if (item.IsActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                if (item.isOriginal == true)
                {
                    isOriginal = "<span onclick='UnsetOriginal(this)' class='status-action label label-success'>Original</span>";
                }
                else
                {
                    isOriginal = "<span onclick='SetOriginal(this)' class='status-action label label-danger'>Copied</span>";
                }
                if (item.isDeleted == true)
                {
                    userdelete = "<span class='status-action label label-danger'>Deleted</span>";
                }
                string actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";

                posts.Add(new DataItem { ItemName = item.Title, Actions = actions, Status = status, UserDelete = userdelete, CommentsCount = commentsCount, ReportedCommentsCount = reportedCommentsCount.ToString(), isOriginal = isOriginal, DT_RowId = item.PostId.ToString() });
            }
            dataTableData.data = posts;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddArticleTopX()
        {
            ViewBag.SubSidebarItem = "lists";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult AddArticleTopX(UserArticleVM article, string article_picture_path, string video_url)
        {
            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            article.newArticle.CreatedBy = currentUser.UserId;
            article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.ActivationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.MetaTags = "";
            List<Tag> tags = _postRepo.getSelectedTags(article.SelectedTags).ToList();
            article.newArticle.Tags = tags;
            HttpPostedFileBase picture = Request.Files[0];
            if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                    if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "Temp/" };
                        string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.newArticle.FeaturedImage = imgName;
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    article.newArticle.FeaturedImage = picName;
                }

                article.newArticle.HasImage = true;
            }
            else if (video_url != null || video_url != string.Empty)
            {
                article.newArticle.HasImage = false;
                article.newArticle.FeaturedVideo = video_url;
                article.newArticle.FeaturedImage = "http://img.youtube.com/vi/" + video_url + "/0.jpg";
            }

            foreach (var item in article.newArticle.ArticleTopXes)
            {
                if (item.TopXImage != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + item.TopXImage;
                    if (item.TopXImage.Contains("youtube"))
                    {
                        string[] video_link = item.TopXImage.Split('/');
                        string video = video_link.ElementAt(video_link.Length - 2);
                        item.TopXVideo = video;
                    } 
                    else if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "Temp/" };
                        string[] temp = item.TopXImage.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        item.TopXImage = imgName;
                    }
                }
            }

            article.newArticle.MetaTags = "";
            article.newArticle.ViewsCount = 0;
            article.newArticle.SharesCount = 0;
            article.newArticle.PostTypeId = 2;
            article.newArticle.IsActive = true;
            Post addedArticle = _postRepo.AddPost(article.newArticle);
            if (addedArticle != null)
            {
                ViewBag.ErrorMsg = 0;
                return RedirectToAction("ArticlesTopX");
            }
            else
            {
                ViewBag.ErrorMsg = 1;
                return View();
            }
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditArticleTopX(int PostId)
        {
            Post article = _postRepo.GetPostById(PostId);
            UserArticleVM articleVM = new UserArticleVM();
            articleVM.newArticle = article;
            ViewBag.SubSidebarItem = "lists";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View(articleVM);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult EditArticleTopX(UserArticleVM articleVM, string article_picture_path, string video_url)
        {
            Post article = _postRepo.GetPostById(articleVM.newArticle.PostId);
            article.Title = articleVM.newArticle.Title;
            article.Content = articleVM.newArticle.Content;
            List<Tag> tags = _postRepo.getSelectedTags(articleVM.SelectedTags).ToList();
            HttpPostedFileBase picture = Request.Files[0];
            if (video_url != null || video_url != string.Empty)
            {
                article.HasImage = false;
                article.FeaturedVideo = video_url;
                article.FeaturedImage = "http://img.youtube.com/vi/" + video_url + "/0.jpg";
            }
            else if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    if (article_picture_path.Contains("Temp"))
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                        if (System.IO.File.Exists(path))
                        {
                            string[] separator = new string[] { "Temp/" };
                            string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                            string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                            System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                            article.FeaturedImage = imgName;
                        }
                    }
                    else
                    {
                        string[] imgName = article_picture_path.Split('/');
                        article.FeaturedImage = imgName.Last();
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    article.FeaturedImage = picName;
                }

                article.HasImage = true;
            }

            var index = 0;
            foreach (var item in articleVM.newArticle.ArticleTopXes)
            {
                if (item.TopXImage != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + item.TopXImage;
                    if(item.TopXImage.Contains("youtube"))
                    {
                        string[] video_link = item.TopXImage.Split('/');
                        string video = video_link.ElementAt(video_link.Length - 2);
                        article.ArticleTopXes.ElementAt(index).TopXVideo = "https://www.youtube.com/watch?v=" + video;
                    } 
                    else if (System.IO.File.Exists(path))
                    {
                        string[] temp = item.TopXImage.Split('/');
                        string imgName = "";
                        if (temp.Last().Contains('_'))
                        {
                            imgName = temp.Last().Split('_').Last();
                        }
                        imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp.Last();
                        
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.ArticleTopXes.ElementAt(index).TopXImage = imgName;
                    }
                }
                index++;
            }

            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            _postRepo.UpdateArticle(article, currentUser);
            _postRepo.UpdatedArticleTags(articleVM.newArticle.PostId, tags);
            return Redirect("/Admin/ArticlesTopX");
        }
        #endregion

        #region Opinions
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Opinions()
        {
            ViewBag.SubSidebarItem = "opinions";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetOpinions(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            int total_rows = _postRepo.GetPosts(3, null).Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> posts = new List<DataItem>();
            IQueryable<Post> posts_items = _postRepo.GetPosts(3, start, length);
            foreach (var item in posts_items)
            {
                string status, isOriginal;
                string userdelete = "";
                string commentsCount = "<span onclick='SeeComments(" + item.PostId + ")' class='comments-count'>" + item.Comments.Count() + "</span>";
                int reportedCommentsCount = item.Comments.Sum(i => i.CommentReportCount);
                if (item.IsActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                if (item.isOriginal == true)
                {
                    isOriginal = "<span onclick='UnsetOriginal(this)' class='status-action label label-success'>Original</span>";
                }
                else
                {
                    isOriginal = "<span onclick='SetOriginal(this)' class='status-action label label-danger'>Copied</span>";
                }
                if (item.isDeleted == true)
                {
                    userdelete = "<span class='status-action label label-danger'>Deleted</span>";
                }
                string actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";

                posts.Add(new DataItem { ItemName = item.Content, Actions = actions, Status = status, UserDelete = userdelete, CommentsCount = commentsCount, ReportedCommentsCount = reportedCommentsCount.ToString(), isOriginal = isOriginal, DT_RowId = item.PostId.ToString() });
            }
            dataTableData.data = posts;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddOpinion()
        {
            ViewBag.SubSidebarItem = "opinions";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult AddOpinion(UserArticleVM article, string article_picture_path, string video_url)
        {
            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            article.newArticle.CreatedBy = currentUser.UserId;
            article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.ActivationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.MetaTags = "";
            List<Tag> tags = _postRepo.getSelectedTags(article.SelectedTags).ToList();
            article.newArticle.Tags = tags;
            HttpPostedFileBase picture = Request.Files[0];
            if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                    if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "Temp/" };
                        string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.newArticle.FeaturedImage = imgName;
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    article.newArticle.FeaturedImage = picName;
                }

                article.newArticle.HasImage = true;
            }
            else if (video_url != null || video_url != string.Empty)
            {
                article.newArticle.HasImage = false;
                article.newArticle.FeaturedVideo = video_url;
            }

            article.newArticle.MetaTags = "";
            article.newArticle.ViewsCount = 0;
            article.newArticle.SharesCount = 0;
            article.newArticle.PostTypeId = 3;
            article.newArticle.IsActive = true;
            Post addedArticle = _postRepo.AddPost(article.newArticle);
            if (addedArticle != null)
            {
                ViewBag.ErrorMsg = 0;
                return RedirectToAction("Opinions");
            }
            else
            {
                ViewBag.ErrorMsg = 1;
                return View();
            }
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditOpinion(int PostId)
        {
            Post opinion = _postRepo.GetPostById(PostId);
            UserArticleVM articleVM = new UserArticleVM();
            articleVM.newArticle = opinion;
            ViewBag.SubSidebarItem = "opinions";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            if (articleVM.newArticle.FeaturedImage == null)
            {
                articleVM.newArticle.FeaturedImage = "";
            }
            return View(articleVM);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult EditOpinion(UserArticleVM articleVM, string article_picture_path, string video_url)
        {
            Post article = _postRepo.GetPostById(articleVM.newArticle.PostId);
            article.Title = articleVM.newArticle.Title;
            article.Content = articleVM.newArticle.Content;
            List<Tag> tags = _postRepo.getSelectedTags(articleVM.SelectedTags).ToList();
            HttpPostedFileBase picture = Request.Files[0];
            if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    if (article_picture_path.Contains("Temp"))
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                        if (System.IO.File.Exists(path))
                        {
                            string[] separator = new string[] { "Temp/" };
                            string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                            string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                            System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                            article.FeaturedImage = imgName;
                        }
                    }
                    else
                    {
                        string[] imgName = article_picture_path.Split('/');
                        article.FeaturedImage = imgName.Last();
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    article.FeaturedImage = picName;
                }

                article.HasImage = true;
            }
            else if (video_url != null || video_url != string.Empty)
            {
                article.HasImage = false;
                article.FeaturedVideo = video_url;
            }
            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            _postRepo.UpdateArticle(article, currentUser);
            _postRepo.UpdatedArticleTags(articleVM.newArticle.PostId, tags);
            return Redirect("/Admin/Articles");
        }
        #endregion

        #region ImagePosts
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ImagePosts()
        {
            ViewBag.SubSidebarItem = "image-posts";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetImagePosts(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            int total_rows = _postRepo.GetPosts(4, null).Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> posts = new List<DataItem>();
            IQueryable<Post> posts_items = _postRepo.GetPosts(4, start, length);
            foreach (var item in posts_items)
            {
                string status, isOriginal;
                string userdelete = "";
                string commentsCount = "<span onclick='SeeComments(" + item.PostId + ")' class='comments-count'>" + item.Comments.Count() + "</span>";
                int reportedCommentsCount = item.Comments.Sum(i => i.CommentReportCount);
                if (item.IsActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                if (item.isOriginal == true)
                {
                    isOriginal = "<span onclick='UnsetOriginal(this)' class='status-action label label-success'>Original</span>";
                }
                else
                {
                    isOriginal = "<span onclick='SetOriginal(this)' class='status-action label label-danger'>Copied</span>";
                }
                if (item.isDeleted == true)
                {
                    userdelete = "<span class='status-action label label-danger'>Deleted</span>";
                }
                string actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";
                string imageHtml = "<img src='/Content/Article_Images/" + item.FeaturedImage + "?w=230&h=140&mode=crop' />";
                posts.Add(new DataItem { ItemName = imageHtml, Actions = actions, Status = status, UserDelete = userdelete, CommentsCount = commentsCount, ReportedCommentsCount = reportedCommentsCount.ToString(), isOriginal = isOriginal, DT_RowId = item.PostId.ToString() });
            }
            dataTableData.data = posts;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddImagePost()
        {
            ViewBag.SubSidebarItem = "image-posts";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult AddImagePost(UserArticleVM article, string article_picture_path, string video_url)
        {
            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            article.newArticle.CreatedBy = currentUser.UserId;
            article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.ActivationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.MetaTags = "";
            List<Tag> tags = _postRepo.getSelectedTags(article.SelectedTags).ToList();
            article.newArticle.Tags = tags;
            HttpPostedFileBase picture = Request.Files[0];
            if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                    if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "Temp/" };
                        string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.newArticle.FeaturedImage = imgName;
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    article.newArticle.FeaturedImage = picName;
                }

                article.newArticle.HasImage = true;
            }
            else if (video_url != null || video_url != string.Empty)
            {
                article.newArticle.HasImage = false;
                article.newArticle.FeaturedVideo = video_url;
            }
            article.newArticle.MetaTags = "";
            article.newArticle.ViewsCount = 0;
            article.newArticle.SharesCount = 0;
            article.newArticle.PostTypeId = 4;
            article.newArticle.IsActive = true;
            article.newArticle.Content = "";
            Post addedArticle = _postRepo.AddPost(article.newArticle);
            if (addedArticle != null)
            {
                ViewBag.ErrorMsg = 0;
                return RedirectToAction("ImagePosts");
            }
            else
            {
                ViewBag.ErrorMsg = 1;
                return View();
            }
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditImagePost(int PostId)
        {
            Post imagePost = _postRepo.GetPostById(PostId);
            UserArticleVM articleVM = new UserArticleVM();
            articleVM.newArticle = imagePost;
            ViewBag.SubSidebarItem = "image-posts";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View(articleVM);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult EditImagePost(UserArticleVM articleVM, string article_picture_path, string video_url)
        {
            Post imagePost = _postRepo.GetPostById(articleVM.newArticle.PostId);
            if (articleVM.newArticle.Content == null)
            {
                imagePost.Content = "";
            }
            else
            {
                imagePost.Content = articleVM.newArticle.Content;
            }
            List<Tag> tags = _postRepo.getSelectedTags(articleVM.SelectedTags).ToList();
            HttpPostedFileBase picture = Request.Files[0];
            if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    if (article_picture_path.Contains("Temp"))
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                        if (System.IO.File.Exists(path))
                        {
                            string[] separator = new string[] { "Temp/" };
                            string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                            string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                            System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                            imagePost.FeaturedImage = imgName;
                        }
                    }
                    else
                    {
                        string[] imgName = article_picture_path.Split('/');
                        imagePost.FeaturedImage = imgName.Last();
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    imagePost.FeaturedImage = picName;
                }

                imagePost.HasImage = true;
            }
            else if (video_url != null || video_url != string.Empty)
            {
                imagePost.HasImage = false;
                imagePost.FeaturedVideo = video_url;
            }
            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            _postRepo.UpdateArticle(imagePost, currentUser);
            _postRepo.UpdatedArticleTags(articleVM.newArticle.PostId, tags);
            return Redirect("/Admin/ImagePosts");
        }
        #endregion

        #region Videos
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Videos()
        {
            ViewBag.SubSidebarItem = "videos";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetVideos(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            int total_rows = _postRepo.GetPosts(5, null).Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> posts = new List<DataItem>();
            IQueryable<Post> posts_items = _postRepo.GetPosts(5, start, length);
            foreach (var item in posts_items)
            {
                string status, isOriginal;
                string userdelete = "";
                string commentsCount = "<span onclick='SeeComments(" + item.PostId + ")' class='comments-count'>" + item.Comments.Count() + "</span>";
                int reportedCommentsCount = item.Comments.Sum(i => i.CommentReportCount);
                if (item.IsActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                if (item.isOriginal == true)
                {
                    isOriginal = "<span onclick='UnsetOriginal(this)' class='status-action label label-success'>Original</span>";
                }
                else
                {
                    isOriginal = "<span onclick='SetOriginal(this)' class='status-action label label-danger'>Copied</span>";
                }
                if (item.isDeleted == true)
                {
                    userdelete = "<span class='status-action label label-danger'>Deleted</span>";
                }
                string actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";
                string videoHtml = "<iframe width='266' height='156' src='https://www.youtube.com/embed/" + item.FeaturedVideo + "' frameborder='0; allowfullscreen=''></iframe>";
                posts.Add(new DataItem { ItemName = videoHtml, Actions = actions, Status = status, UserDelete = userdelete, CommentsCount = commentsCount, ReportedCommentsCount = reportedCommentsCount.ToString(), isOriginal = isOriginal, DT_RowId = item.PostId.ToString() });
            }
            dataTableData.data = posts;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AddVideo()
        {
            ViewBag.SubSidebarItem = "videos";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult AddVideo(UserArticleVM article, string article_picture_path, string video_url)
        {
            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            article.newArticle.CreatedBy = currentUser.UserId;
            article.newArticle.CreationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.ActivationDate = DateTime.UtcNow.ToLocalTime();
            article.newArticle.MetaTags = "";
            List<Tag> tags = _postRepo.getSelectedTags(article.SelectedTags).ToList();
            article.newArticle.Tags = tags;
            HttpPostedFileBase picture = Request.Files[0];
            if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                    if (System.IO.File.Exists(path))
                    {
                        string[] separator = new string[] { "Temp/" };
                        string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                        string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                        System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                        article.newArticle.FeaturedImage = imgName;
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    article.newArticle.FeaturedImage = picName;
                }

                article.newArticle.HasImage = true;
            }
            else if (video_url != null || video_url != string.Empty)
            {
                article.newArticle.HasImage = false;
                article.newArticle.FeaturedVideo = video_url;
            }

            article.newArticle.MetaTags = "";
            article.newArticle.ViewsCount = 0;
            article.newArticle.SharesCount = 0;
            article.newArticle.PostTypeId = 4;
            article.newArticle.IsActive = true;
            article.newArticle.Content = "";
            Post addedArticle = _postRepo.AddPost(article.newArticle);
            if (addedArticle != null)
            {
                ViewBag.ErrorMsg = 0;
                return RedirectToAction("Videos");
            }
            else
            {
                ViewBag.ErrorMsg = 1;
                return View();
            }
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult EditVideo(int PostId)
        {
            Post video = _postRepo.GetPostById(PostId);
            UserArticleVM articleVM = new UserArticleVM();
            articleVM.newArticle = video;
            ViewBag.SubSidebarItem = "videos";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View(articleVM);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult EditVideo(UserArticleVM articleVM, string article_picture_path, string video_url)
        {
            Post video = _postRepo.GetPostById(articleVM.newArticle.PostId);
            if (articleVM.newArticle.Content == null)
            {
                video.Content = "";
            }
            else
            {
                video.Content = articleVM.newArticle.Content;
            }
            List<Tag> tags = _postRepo.getSelectedTags(articleVM.SelectedTags).ToList();
            HttpPostedFileBase picture = Request.Files[0];
            if (picture.FileName != "" || article_picture_path != "")
            {
                if (article_picture_path != "")
                {
                    if (article_picture_path.Contains("Temp"))
                    {
                        string path = AppDomain.CurrentDomain.BaseDirectory + article_picture_path;
                        if (System.IO.File.Exists(path))
                        {
                            string[] separator = new string[] { "Temp/" };
                            string[] temp = article_picture_path.Split(separator, StringSplitOptions.None);
                            string imgName = DateTime.UtcNow.ToLocalTime().Ticks + "_" + temp[1];
                            System.IO.File.Copy(path, Server.MapPath("~/Content/Article_Images/" + imgName));
                            video.FeaturedImage = imgName;
                        }
                    }
                    else
                    {
                        string[] imgName = article_picture_path.Split('/');
                        video.FeaturedImage = imgName.Last();
                    }
                }
                else if (picture.FileName != "")
                {
                    string picName = System.IO.Path.GetFileName(picture.FileName);
                    string path = System.IO.Path.Combine(Server.MapPath("~/Content/Article_Images"), picName);
                    picture.SaveAs(path);
                    video.FeaturedImage = picName;
                }

                video.HasImage = true;
            }
            else if (video_url != null || video_url != string.Empty)
            {
                video.HasImage = false;
                video.FeaturedVideo = video_url;
            }
            RWSUser currentUser = _userRepo.GetUserByUsername(User.Identity.Name);
            _postRepo.UpdateArticle(video, currentUser);
            _postRepo.UpdatedArticleTags(articleVM.newArticle.PostId, tags);
            return Redirect("/Admin/Videos");
        }
        #endregion

        #region Comments
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult PostComments(int PostId, int? Page)
        {
            int post_type = _postRepo.GetPostById(PostId).PostTypeId.Value;
            if (post_type == 1)
            {
                ViewBag.SubSidebarItem = "articles";
            }
            else if (post_type == 2)
            {
                ViewBag.SubSidebarItem = "lists";
            }
            else if (post_type == 3)
            {
                ViewBag.SubSidebarItem = "opinions";
            }
            else if (post_type == 4)
            {
                ViewBag.SubSidebarItem = "image-posts";
            }
            else if (post_type == 5)
            {
                ViewBag.SubSidebarItem = "videos";
            }
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            ViewBag.AllCommentsCount = _postRepo.GetAllCommentsCount(PostId, null, null, null, null);
            if (Page == null) Page = 0;
            List<Comment> comments = _postRepo.GetComments(Page.Value, PostId, null, null, null, null).ToList();
            comments.Reverse();
            if (Request.IsAjaxRequest())
            {
                return PartialView("_PostCommentPartial", comments);
            }
            return View(comments);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteComment(int CommentId)
        {
            Comment comment = _postRepo.GetCommentById(CommentId);
            _postRepo.DeleteComment(comment);
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Engagements
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult PointTypes()
        {
            ViewBag.SubSidebarItem = "Point Types";
            ViewBag.SidebarItem = "point-types-management";
            ViewBag.PageHeader = "Point Types Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetPointTypes(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            int total_rows = _engRepo.GetPointTypes(null, null).Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> point_types = new List<DataItem>();
            IQueryable<PointType> point_type_items = _engRepo.GetPointTypes(start, length);
            foreach (var item in point_type_items)
            {
                string actions = "<a href='#' onclick='Edit(this);return false;'><i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";
                //string actions = "<a href='#' onclick='Edit(this);return false;'><i class='fa fa-pencil'></i></a>";
                string savebtn = "<button class='btn btn-primary btn-sm' onclick='Save(this)'>Save</button>";
                point_types.Add(new DataItem { ItemName = item.PointTypeName, articlesCount = item.PointTypeWeight.Value, Actions = actions, Status = savebtn, DT_RowId = item.PointTypeId.ToString() });
            }
            dataTableData.data = point_types;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult DeletePointType(int PointTypeId)
        {
            _engRepo.DeletePointType(PointTypeId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [CustomAuthorize(Roles = "Admin")]
        [ValidateInput(false)]
        public ActionResult EditPointType(int PointTypeId, double PointTypeWeight)
        {
            //EngagementType EngType = _postRepo.GetEngagementTypeById(EngagementTypeId);
            //EngType.EngTypeId = EngagementTypeNewId;
            //EngType.EngType = EngagementType;
            //EngType.EngWeight = EngagementWeight;
            //EngType = _postRepo.UpdateEngagementType(EngType);
            PointType PointType = _engRepo.UpdatePointType(PointTypeId, PointTypeWeight);
            dynamic data = new System.Dynamic.ExpandoObject();
            //data.PointType = EngType.EngType;
            data.PointTypeWeight = PointType.PointTypeWeight;
            data.PointTypeId = PointType.PointTypeId;
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Reports
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Leaderboard()
        {
            ViewBag.SubSidebarItem = "leaderboard";
            ViewBag.SidebarItem = "reports-management";
            ViewBag.PageHeader = "Reports";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AjaxGetLeaderboard(int draw, int start, int length, string filter, string fromDate, string toDate)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            
            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            List<DataItem> data = new List<DataItem>();

            if (filter == "week")
            {
                DateTime WeekStartDate = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
                DateTime WeekEndDate = WeekStartDate.AddDays(6);
                List<int> LeaderIdsWeekly = _userRepo.GetLeaderboardAuthorIds(WeekStartDate, WeekEndDate);
                //List<UserPointsVM> WeeklyLeadersPoints = new List<UserPointsVM>();
                foreach (var item in LeaderIdsWeekly)
                {
                    //WeeklyLeadersPoints.Add(new UserPointsVM { UserId = item, UserProfilePicture = _userRepo.GetUserByUserId(item).ProfileImagePath, UserName = _userRepo.GetUserByUserId(item).UserName, PointsValue = _userRepo.GetUserPointsBySelectedDate(item, WeekStartDate, WeekEndDate) });
                    data.Add(new DataItem { ItemName = _userRepo.GetUserByUserId(item).UserName, articlesCount = _userRepo.GetUserPointsBySelectedDate(item, WeekStartDate, WeekEndDate), DT_RowId = item.ToString() });
                }
                dataTableData.data = data;
            }
            else if (filter == "month")
            {
                DateTime MonthStartDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, 1);
                DateTime MonthEndDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, DateTime.DaysInMonth(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month));
                List<int> LeaderIdsMonthly = _userRepo.GetLeaderboardAuthorIds(MonthStartDate, MonthEndDate);
                foreach (var item in LeaderIdsMonthly)
                {
                    data.Add(new DataItem { ItemName = _userRepo.GetUserByUserId(item).UserName, articlesCount = _userRepo.GetUserPointsBySelectedDate(item, MonthStartDate, MonthEndDate), DT_RowId = item.ToString() });
                }  
                dataTableData.data = data;
            }
            else if (filter == "alltime")
            {
                List<int> LeaderIdsAllTime = _userRepo.GetLeaderboardAuthorIds(null, null);
                List<UserPointsVM> AllTimeLeadersPoints = new List<UserPointsVM>();
                foreach (var item in LeaderIdsAllTime)
                {
                    data.Add(new DataItem { ItemName = _userRepo.GetUserByUserId(item).UserName, articlesCount = _userRepo.GetUserPointsBySelectedDate(item, null, null), DT_RowId = item.ToString() });
                }
                dataTableData.data = data;
            }
            else if (filter == "custom")
            {
                DateTime? from = null;
                DateTime? to = null;
                string[] from_date, to_date;
                if (fromDate != "" && fromDate != null)
                {
                    from_date = fromDate.Split('/');
                    from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
                }
                if (toDate != "" && toDate != null)
                {
                    to_date = toDate.Split('/');
                    to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
                }

                List<int> LeaderIdsMonthly = _userRepo.GetLeaderboardAuthorIds(from, to);
                foreach (var item in LeaderIdsMonthly)
                {
                    data.Add(new DataItem { ItemName = _userRepo.GetUserByUserId(item).UserName, articlesCount = _userRepo.GetUserPointsBySelectedDate(item, from, to), DT_RowId = item.ToString() });
                }
                dataTableData.data = data;
            }
            int total_rows = data.Count();
            if (length == -1)
            {
                length = total_rows;
            }
            dataTableData.recordsTotal = total_rows;
            dataTableData.recordsFiltered = total_rows;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public MemoryStream GetStream(XLWorkbook excelWorkbook)
        {
            MemoryStream fs = new MemoryStream();
            excelWorkbook.SaveAs(fs);
            fs.Position = 0;
            return fs;
        }

        [CustomAuthorize(Roles = "Admin")]
        public void Export(string filter, string fromDate, string toDate)
        {
            List<DataItem> data = new List<DataItem>();
            XLWorkbook wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("RWS Leaderboard");
            string fileNamePrefix = "";

            ws.Cell("B2").Value = "Raya W Soffara Leaderboard";

            if (filter == "week")
            {
                fileNamePrefix = "Week_Leaderboard_";
                DateTime WeekStartDate = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
                DateTime WeekEndDate = WeekStartDate.AddDays(6);
                List<int> LeaderIdsWeekly = _userRepo.GetLeaderboardAuthorIds(WeekStartDate, WeekEndDate);
                foreach (var item in LeaderIdsWeekly)
                {
                    data.Add(new DataItem { ItemName = _userRepo.GetUserByUserId(item).UserName, articlesCount = _userRepo.GetUserPointsBySelectedDate(item, WeekStartDate, WeekEndDate), DT_RowId = item.ToString() });
                }
                ws.Cell("C3").Value = "From:  " + WeekStartDate;
                ws.Cell("D3").Value = "To:  " + WeekEndDate;
            }
            else if (filter == "month")
            {
                fileNamePrefix = "Month_Leaderboard_";
                DateTime MonthStartDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, 1);
                DateTime MonthEndDate = new DateTime(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month, DateTime.DaysInMonth(DateTime.UtcNow.ToLocalTime().Year, DateTime.UtcNow.ToLocalTime().Month));
                List<int> LeaderIdsMonthly = _userRepo.GetLeaderboardAuthorIds(MonthStartDate, MonthEndDate);
                foreach (var item in LeaderIdsMonthly)
                {
                    data.Add(new DataItem { ItemName = _userRepo.GetUserByUserId(item).UserName, articlesCount = _userRepo.GetUserPointsBySelectedDate(item, MonthStartDate, MonthEndDate), DT_RowId = item.ToString() });
                }
                ws.Cell("C3").Value = "From:  " + MonthStartDate;
                ws.Cell("D3").Value = "To:  " + MonthEndDate;
            }
            else if (filter == "alltime")
            {
                fileNamePrefix = "AllTime_Leaderboard_";
                List<int> LeaderIdsAllTime = _userRepo.GetLeaderboardAuthorIds(null, null);
                List<UserPointsVM> AllTimeLeadersPoints = new List<UserPointsVM>();
                foreach (var item in LeaderIdsAllTime)
                {
                    data.Add(new DataItem { ItemName = _userRepo.GetUserByUserId(item).UserName, articlesCount = _userRepo.GetUserPointsBySelectedDate(item, null, null), DT_RowId = item.ToString() });
                }
                ws.Cell("C3").Value = "All Time";
            }
            else if (filter == "custom")
            {
                fileNamePrefix = "Custom_Leaderboard_";
                DateTime? from = null;
                DateTime? to = null;
                string[] from_date, to_date;
                if (fromDate != null && fromDate != "")
                {
                    from_date = fromDate.Split('/');
                    from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
                }
                if (toDate != null && toDate != "")
                {
                    to_date = toDate.Split('/');
                    to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
                }

                List<int> LeaderIdsMonthly = _userRepo.GetLeaderboardAuthorIds(from, to);
                foreach (var item in LeaderIdsMonthly)
                {
                    data.Add(new DataItem { ItemName = _userRepo.GetUserByUserId(item).UserName, articlesCount = _userRepo.GetUserPointsBySelectedDate(item, from, to), DT_RowId = item.ToString() });
                }
                ws.Cell("C3").Value = "From:  " + from;
                ws.Cell("D3").Value = "To:  " + to;
            }

            ws.Cell("C4").Value = "User Name";
            ws.Cell("D4").Value = "Points";
            int rowNum = 5;

            foreach (var item in data)
            {
                ws.Cell("C" + rowNum).Value = item.ItemName;
                ws.Cell("D" + rowNum).Value = item.articlesCount;
                rowNum++;
            }

            // From worksheet
            var rngTable = ws.Range("B2:C6");

            // From another range
            //var rngDates = rngTable.Range("D3:D5"); // The address is relative to rngTable (NOT the worksheet)
            //var rngNumbers = rngTable.Range("E3:E5"); // The address is relative to rngTable (NOT the worksheet)

            // Using OpenXML's predefined formats
            //rngDates.Style.NumberFormat.NumberFormatId = 15;

            // Using a custom format
            //rngNumbers.Style.NumberFormat.Format = "$ #,##0";

            var rngHeaders = ws.Range("B2:C2"); // The address is relative to rngTable (NOT the worksheet)
            rngHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngHeaders.Style.Font.Bold = true;
            rngHeaders.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;

            ws.Range("A3:F3").Style.Font.Bold = true;
            ws.Range("A3:F3").Style.Fill.BackgroundColor = XLColor.Aqua;
            ws.Range("A3:F3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range("A4:F4").Style.Fill.BackgroundColor = XLColor.BlizzardBlue;

            ws.Row(1).Merge(); // We could've also used: rngTable.Range("A1:E1").Merge()
            ws.Row(2).Merge();

            ws.Columns(2, 6).AdjustToContents();

            string fileName = Server.UrlEncode(fileNamePrefix + DateTime.UtcNow.ToLocalTime().ToShortDateString().Replace("/", "_") + ".xlsx");
            MemoryStream stream = GetStream(wb);

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/vnd.ms-excel";
            Response.BinaryWrite(stream.ToArray());
            Response.End();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult UserPoints() {
            ViewBag.SubSidebarItem = "user-points";
            ViewBag.SidebarItem = "reports-management";
            ViewBag.PageHeader = "Reports";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult GetUserNames()
        {
            List<Item> users = new List<Item>();
            IQueryable<RWSUser> usersList = _userRepo.GetAllActiveUsers(null, null);
            foreach (var item in usersList)
            {
                users.Add(new Item { ItemId = item.UserId, ItemName = item.UserName });
            }
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AjaxGetUserPoints(int draw, int start, int length, string userName, string fromDate, string toDate)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            List<DataItem> data = new List<DataItem>();

            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != "" && fromDate != null)
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != "" && toDate != null)
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }

            DateTime WeekStartDate = DateTime.Today.AddDays(-1 * (int)(DateTime.Today.DayOfWeek));
            DateTime WeekEndDate = WeekStartDate.AddDays(6);

            int userId = _userRepo.GetUserByUsername(userName).UserId;
            double customPoints = _userRepo.GetUserPointsBySelectedDate(userId, from, to);
            double weekPoints = _userRepo.GetUserPointsBySelectedDate(userId, WeekStartDate, WeekEndDate);
            double monthPoints = _userRepo.GetUserPointsByMonthId(userId, DateTime.UtcNow.ToLocalTime().Month, DateTime.UtcNow.ToLocalTime().Year);
            double alltimePoints = _userRepo.GetUserPointsBySelectedDate(userId, null, null);

            data.Add(new DataItem { ItemName = userName, Actions = weekPoints.ToString(), articlesCount = monthPoints, Featured = alltimePoints.ToString(), Status = customPoints.ToString(), DT_RowId = userId.ToString() });
            dataTableData.data = data;
            
            int total_rows = data.Count();
            if (length == -1)
            {
                length = total_rows;
            }
            dataTableData.recordsTotal = total_rows;
            dataTableData.recordsFiltered = total_rows;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ReportUsers()
        {
            ViewBag.SubSidebarItem = "report-users";
            ViewBag.SidebarItem = "reports-management";
            ViewBag.PageHeader = "Reports";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AjaxGetReportUsers(int draw, int start, int length, string status, string fromDate, string toDate)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.Form["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.Form["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            int displayedNum, recordsFiltered;
            List<DataItem> userprofiles = new List<DataItem>();
            int pageNum = start / length;

            IQueryable<RWSUser> users = null;
            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != "" && fromDate != null)
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != "" && toDate != null)
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }
            if (search == "" || search == null)
            {
                users = _userRepo.GetAllUsers(pageNum, length, status, from, to, out displayedNum);
            }
            else
            {
                users = _userRepo.GetUsersBySearchTerm(start, length, search, status, from, to);
            }

            int total_rows = _userRepo.GetUsersCount(status, search, from, to);
            if (length == -1)
            {
                length = total_rows;
            }
            dataTableData.recordsTotal = total_rows;
            recordsFiltered = total_rows;

            foreach (var item in users)
            {
                string creation = "", activation = "", active = "", inactive = "", points = "";
                string userstatus = "<span class='label label-danger'>Inactive</span>";
                
                if(item.ConfirmationDate != null){
                    activation = item.ConfirmationDate.Value.ToShortDateString();
                }
                if (item.CreationDate != null)
                {
                    creation = item.CreationDate.Value.ToShortDateString();
                }
                if(item.IsConfirmed != null){
                    userstatus = (item.IsConfirmed.Value ? "<span class='label label-success'>Active</span>" : "<span class='label label-danger'>Inactive</span>");
                }

                points = "<a href='/Admin/ReportPosts?username=" + item.UserName + "&status=Points'>" + _userRepo.GetUserPointsBySelectedDate(item.UserId, null, null) + "</a>";
                active = "<a href='/Admin/ReportPosts?username=" + item.UserName + "&status=Active'>" + _userRepo.GetUserPostsCount(item.UserId, "Active") + "</a>";
                inactive = "<a href='/Admin/ReportPosts?username=" + item.UserName + "&status=Inactive'>" + _userRepo.GetUserPostsCount(item.UserId, "Inactive") + "</a>";
                userprofiles.Add(new DataItem { ItemName = item.UserName, 
                    CreationDate = creation,
                    ActivationDate = activation, 
                    Email = item.Email, 
                    Status = userstatus, 
                    Points = points,
                    ActivePosts = active,  
                    InactivePosts = inactive, 
                    DT_RowId = item.UserId.ToString() });
            }

            dataTableData.data = userprofiles;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public void ExportUsers(string status, string fromDate, string toDate, string search)
        {
            List<DataItem> data = new List<DataItem>();
            XLWorkbook wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("RWS Users");
            string fileNamePrefix = "";

            ws.Cell("B2").Value = "Raya W Soffara " + status + " Users";

            fileNamePrefix = "RWS_Users_";
            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != null && fromDate != "")
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != null && toDate != "")
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }
            IQueryable<RWSUser> users;
            int displayedNum;
            if (search == "" || search == null)
            {
                users = _userRepo.GetAllUsers(null, null, status, from, to, out displayedNum);
            }
            else
            {
                users = _userRepo.GetUsersBySearchTerm(null, null, search, status, from, to);
            }

            foreach (var item in users)
            {
                string activation = "";
                if (item.ConfirmationDate != null)
                {
                    activation = item.ConfirmationDate.Value.ToShortDateString();
                }
                data.Add(new DataItem { ItemName = item.UserName, ActivationDate = activation, Email = item.Email, Status = (item.IsConfirmed.Value ? "Active" : "Inactive"), Points = _userRepo.GetUserPointsBySelectedDate(item.UserId, null, null).ToString(), ActivePosts = _userRepo.GetUserPostsCount(item.UserId, "Active").ToString(), InactivePosts = _userRepo.GetUserPostsCount(item.UserId, "Inactive").ToString(), DT_RowId = item.UserId.ToString() });
            }

            ws.Cell("C3").Value = "From:  " + from;
            ws.Cell("D3").Value = "To:  " + to;

            ws.Cell("A4").Value = "Id";
            ws.Cell("B4").Value = "User Name";
            ws.Cell("C4").Value = "Activation Date";
            ws.Cell("D4").Value = "Email";
            ws.Cell("E4").Value = "Status"; 
            ws.Cell("F4").Value = "Total Points";
            ws.Cell("G4").Value = "Active Posts";
            ws.Cell("H4").Value = "Active Posts";
            int rowNum = 5;

            foreach (var item in data)
            {
                ws.Cell("A" + rowNum).Value = item.DT_RowId;
                ws.Cell("B" + rowNum).Value = item.ItemName;
                ws.Cell("C" + rowNum).Value = item.ActivationDate;
                ws.Cell("D" + rowNum).Value = item.Email;
                ws.Cell("E" + rowNum).Value = item.Status;
                ws.Cell("F" + rowNum).Value = item.Points;
                ws.Cell("G" + rowNum).Value = item.ActivePosts;
                ws.Cell("H" + rowNum).Value = item.InactivePosts;
                rowNum++;
            }

            // From worksheet
            var rngTable = ws.Range("B2:C6");

            var rngHeaders = ws.Range("B2:C2"); // The address is relative to rngTable (NOT the worksheet)
            rngHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngHeaders.Style.Font.Bold = true;
            rngHeaders.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;

            ws.Range("A3:H3").Style.Font.Bold = true;
            ws.Range("A3:H3").Style.Fill.BackgroundColor = XLColor.Aqua;
            ws.Range("A3:H3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range("A4:H4").Style.Fill.BackgroundColor = XLColor.BlizzardBlue;

            ws.Row(1).Merge(); // We could've also used: rngTable.Range("A1:E1").Merge()
            ws.Row(2).Merge();

            ws.Columns(2, 6).AdjustToContents();

            string fileName = Server.UrlEncode(fileNamePrefix + DateTime.UtcNow.ToLocalTime().ToShortDateString().Replace("/", "_") + ".xlsx");
            MemoryStream stream = GetStream(wb);

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/vnd.ms-excel";
            Response.BinaryWrite(stream.ToArray());
            Response.End();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ReportPosts()
        {
            ViewBag.SubSidebarItem = "report-posts";
            ViewBag.SidebarItem = "reports-management";
            ViewBag.PageHeader = "Reports";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AjaxGetReportPosts(int draw, int start, int length, string status, string fromDate, string toDate, string username)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.Form["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.Form["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            int displayedNum, recordsFiltered;
            List<DataItem> postprofiles = new List<DataItem>();
            int pageNum = start / length;

            IQueryable<Post> posts = null;
            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != "" && fromDate != null)
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != "" && toDate != null)
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }
            
            int total_rows = _postRepo.GetPostsCount(status, search, from, to, username);
            if (length == -1)
            {
                length = total_rows;
            }
            dataTableData.recordsTotal = total_rows;
            recordsFiltered = total_rows;
            posts = _postRepo.GetPosts(pageNum, search, status, from, to, username, length);
            foreach (var item in posts)
            {
                string activation = "", creation = "", media = "", tags = "";
                double points = 0;
                string description = TrimTextHelper.TrimText(item.Content, 50);
                string poststatus = "<span class='label label-danger'>Inactive</span>";
                string isOriginal = "<span class='label label-default'>Copied</span>";

                if (item.ActivationDate != null)
                {
                    activation = item.ActivationDate.Value.ToShortDateString();
                }
                if (item.CreationDate != null)
                {
                    creation = item.CreationDate.Value.ToShortDateString();
                }
                IQueryable<Tag> posttags = _postRepo.GetTagsForPost(item.PostId);
                foreach (var posttag in posttags)
                {
                    tags += posttag.TagName + ",";
                }
                if (tags != null && tags != "")
                {
                    tags = tags.Substring(0, tags.Length - 1);
                }
                if (item.FeaturedVideo != "" && item.FeaturedVideo != null)
                {
                    media = "<a target='_blank' href='https://www.youtube.com/watch?v=" + item.FeaturedVideo + "'>" + item.FeaturedVideo + "</a>";
                }
                else if (item.FeaturedImage != "" && item.FeaturedImage != null)
                {
                    media = "<a target='_blank' href='/Content/Article_Images/" + item.FeaturedImage + "'>" + item.FeaturedImage + "</a>";
                }
                string viewsValue = _engRepo.GetEngValueByPostId(item.PostId, 3).ToString();
                string likesValue = _engRepo.GetEngValueByPostId(item.PostId, 2).ToString();
                string sharesValue = _engRepo.GetEngValueByPostId(item.PostId, 1).ToString();
                string views = _engRepo.GetEngCountByPostId(item.PostId, 3).ToString();
                string likes = _engRepo.GetEngCountByPostId(item.PostId, 2).ToString();
                string shares = _engRepo.GetEngCountByPostId(item.PostId, 1).ToString();
                string comments = "<a href='/Admin/ReportComments?PostId=" + item.PostId + "&status=Nonreported'>" + _postRepo.GetAllComments(item.PostId).Count() + "</a>";
                string reportedcomments = "<a href='/Admin/ReportComments?PostId=" + item.PostId + "&status=Reported'>" + _postRepo.GetAllCommentsCount(item.PostId, null, "Reported", null, null) + "</a>";
                IQueryable<PointsView> pointviews = _engRepo.GetPostPoints(item.PostId);
                if (pointviews.Count() > 0)
                {
                    points = pointviews.Sum(i => i.PointTypeWeight.Value);
                }
                if (item.IsActive != null)
                {
                    poststatus = item.IsActive.Value ? "<span class='label label-success'>Active</span>" : "<span class='label label-danger'>Inactive</span>";
                    if (item.isDeleted == true)
                    {
                        poststatus = "<span class='label label-default'>Deleted</span>";
                    }
                }
                if (item.isOriginal != null)
                {
                    isOriginal = item.isOriginal.Value ? "<span class='label label-success'>Original</span>" : "<span class='label label-default'>Copied</span>";
                }
                postprofiles.Add(new DataItem { 
                    ItemName = item.Title, 
                    Description = description,
                    Media = media,
                    Actions = item.PostType.PostTypeName,
                    Email = item.RWSUser.UserName,
                    Tags = tags,
                    CreationDate = creation,
                    ActivationDate = activation,
                    Status = poststatus,
                    ViewsCount = views,
                    LikesCount = likes,
                    SharesCount = shares,
                    ViewsValue = viewsValue,
                    LikesValue = likesValue,
                    SharesValue = sharesValue,
                    isOriginal = isOriginal,
                    Points = points.ToString(), 
                    CommentsCount = comments,
                    ReportedCommentsCount = reportedcomments,
                    DT_RowId = item.PostId.ToString() });
            }

            dataTableData.data = postprofiles;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);

        }

        [CustomAuthorize(Roles = "Admin")]
        public void ExportPosts(string status, string fromDate, string toDate, string search, string username)
        {
            List<DataItem> data = new List<DataItem>();
            XLWorkbook wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("RWS Posts");
            string fileNamePrefix = "";

            ws.Cell("B2").Value = "Raya W Soffara " + status + " Posts";

            fileNamePrefix = "RWS_Posts_";
            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != null && fromDate != "")
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != null && toDate != "")
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }
            IQueryable<Post> posts;
            posts = _postRepo.GetPosts(null, search, status, from, to, username, null);

            foreach (var item in posts)
            {
                string activation = "", creation = "", media = "", tags = "";
                double points = 0;
                string description = TrimTextHelper.TrimText(item.Content, 50);
                string poststatus = "Inactive";
                string isOriginal = "Copied";

                if (item.ActivationDate != null)
                {
                    activation = item.ActivationDate.Value.ToShortDateString();
                }
                if (item.CreationDate != null)
                {
                    creation = item.CreationDate.Value.ToShortDateString();
                }
                IQueryable<Tag> posttags = _postRepo.GetTagsForPost(item.PostId);
                foreach (var posttag in posttags)
                {
                    tags += posttag.TagName + ",";
                }
                if (tags != null && tags != "")
                {
                    tags = tags.Substring(0, tags.Length - 1);
                }
                if (item.FeaturedVideo != "" && item.FeaturedVideo != null)
                {
                    media = "https://www.youtube.com/watch?v=" + item.FeaturedVideo;
                }
                else if (item.FeaturedImage != "" && item.FeaturedImage != null)
                {
                    media = Request.Url.GetLeftPart(UriPartial.Authority) + "/Content/Article_Images/" + item.FeaturedImage;
                }
                string viewsValue = _engRepo.GetEngValueByPostId(item.PostId, 3).ToString();
                string likesValue = _engRepo.GetEngValueByPostId(item.PostId, 2).ToString();
                string sharesValue = _engRepo.GetEngValueByPostId(item.PostId, 1).ToString();
                string views = _engRepo.GetEngCountByPostId(item.PostId, 3).ToString();
                string likes = _engRepo.GetEngCountByPostId(item.PostId, 2).ToString();
                string shares = _engRepo.GetEngCountByPostId(item.PostId, 1).ToString();
                string comments = _postRepo.GetAllComments(item.PostId).Count().ToString();
                string reportedcomments = _postRepo.GetAllCommentsCount(item.PostId, null, "Reported", null, null).ToString();
                IQueryable<PointsView> pointviews = _engRepo.GetPostPoints(item.PostId);
                if (pointviews.Count() > 0)
                {
                    points = pointviews.Sum(i => i.PointTypeWeight.Value);
                }
                if (item.IsActive != null)
                {
                    poststatus = item.IsActive.Value ? "Active" : "Inactive";
                    if (item.isDeleted == true)
                    {
                        poststatus = "Deleted";
                    }
                }
                if (item.isOriginal != null)
                {
                    isOriginal = item.isOriginal.Value ? "Original" : "Copied";
                }
                data.Add(new DataItem { 
                    ItemName = item.Title, 
                    Description = description,
                    Media = media,
                    Actions = item.PostType.PostTypeName,
                    Email = item.RWSUser.UserName,
                    Tags = tags,
                    CreationDate = creation,
                    ActivationDate = activation,
                    Status = poststatus,
                    ViewsCount = views,
                    LikesCount = likes,
                    SharesCount = shares,
                    ViewsValue = viewsValue,
                    LikesValue = likesValue,
                    SharesValue = sharesValue,
                    isOriginal = isOriginal,
                    Points = points.ToString(), 
                    CommentsCount = comments,
                    ReportedCommentsCount = reportedcomments,
                    DT_RowId = item.PostId.ToString() });
            }

            ws.Cell("C3").Value = "From:  " + from;
            ws.Cell("D3").Value = "To:  " + to;

            ws.Cell("A4").Value = "Id";
            ws.Cell("B4").Value = "Title";
            ws.Cell("C4").Value = "Content";
            ws.Cell("D4").Value = "Media";
            ws.Cell("E4").Value = "Type";
            ws.Cell("F4").Value = "Username";
            ws.Cell("G4").Value = "Tags";
            ws.Cell("H4").Value = "Creation Date";
            ws.Cell("I4").Value = "Activation Date";
            ws.Cell("J4").Value = "Status";
            ws.Cell("K4").Value = "Views";
            ws.Cell("L4").Value = "Likes";
            ws.Cell("M4").Value = "Shares";
            ws.Cell("N4").Value = "Views Value";
            ws.Cell("O4").Value = "Likes Value";
            ws.Cell("P4").Value = "Shares Value";
            ws.Cell("Q4").Value = "Original";
            ws.Cell("R4").Value = "Points";
            ws.Cell("S4").Value = "Comments";
            ws.Cell("T4").Value = "Reported Comments";
            int rowNum = 5;

            foreach (var item in data)
            {
                ws.Cell("A" + rowNum).Value = item.DT_RowId;
                ws.Cell("B" + rowNum).Value = item.ItemName;
                ws.Cell("C" + rowNum).Value = item.Description;
                ws.Cell("D" + rowNum).Value = item.Media;
                ws.Cell("E" + rowNum).Value = item.Actions;
                ws.Cell("F" + rowNum).Value = item.Email;
                ws.Cell("G" + rowNum).Value = item.Tags;
                ws.Cell("H" + rowNum).Value = item.CreationDate;
                ws.Cell("I" + rowNum).Value = item.ActivationDate;
                ws.Cell("J" + rowNum).Value = item.Status;
                ws.Cell("K" + rowNum).Value = item.ViewsCount;
                ws.Cell("L" + rowNum).Value = item.LikesCount;
                ws.Cell("M" + rowNum).Value = item.SharesCount;
                ws.Cell("N" + rowNum).Value = item.ViewsValue;
                ws.Cell("O" + rowNum).Value = item.LikesValue;
                ws.Cell("P" + rowNum).Value = item.SharesValue;
                ws.Cell("Q" + rowNum).Value = item.isOriginal;
                ws.Cell("R" + rowNum).Value = item.Points;
                ws.Cell("S" + rowNum).Value = item.CommentsCount;
                ws.Cell("T" + rowNum).Value = item.ReportedCommentsCount;
                rowNum++;
            }

            // From worksheet
            var rngTable = ws.Range("B2:C6");

            var rngHeaders = ws.Range("B2:C2"); // The address is relative to rngTable (NOT the worksheet)
            rngHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngHeaders.Style.Font.Bold = true;
            rngHeaders.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;

            ws.Range("A3:T3").Style.Font.Bold = true;
            ws.Range("A3:T3").Style.Fill.BackgroundColor = XLColor.Aqua;
            ws.Range("A3:T3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range("A4:T4").Style.Fill.BackgroundColor = XLColor.BlizzardBlue;

            ws.Row(1).Merge(); // We could've also used: rngTable.Range("A1:E1").Merge()
            ws.Row(2).Merge();

            ws.Columns(2, 6).AdjustToContents();

            string fileName = Server.UrlEncode(fileNamePrefix + DateTime.UtcNow.ToLocalTime().ToShortDateString().Replace("/", "_") + ".xlsx");
            MemoryStream stream = GetStream(wb);

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/vnd.ms-excel";
            Response.BinaryWrite(stream.ToArray());
            Response.End();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult ReportComments()
        {
            ViewBag.SubSidebarItem = "report-comments";
            ViewBag.SidebarItem = "reports-management";
            ViewBag.PageHeader = "Reports";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AjaxGetReportComments(int draw, int start, int length, string status, string fromDate, string toDate, int? PostId)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.Form["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.Form["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            int displayedNum, recordsFiltered;
            List<DataItem> commentsprofile = new List<DataItem>();
            int pageNum = start / length;

            IQueryable<Comment> comments = null;
            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != "" && fromDate != null)
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != "" && toDate != null)
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }

            int total_rows = _postRepo.GetAllCommentsCount(PostId, search, status, from, to);
            if (length == -1)
            {
                length = total_rows;
            }
            dataTableData.recordsTotal = total_rows;
            recordsFiltered = total_rows;
            comments = _postRepo.GetComments(pageNum, PostId, search, status, from, to, length);
            foreach (var item in comments)
            {
                string media = "";
                if (item.Post.FeaturedVideo != "" && item.Post.FeaturedVideo != null)
                {
                    media = "<a target='_blank' href='https://www.youtube.com/watch?v=" + item.Post.FeaturedVideo + "'>" + item.Post.FeaturedVideo + "</a>";
                }
                else if (item.Post.FeaturedImage != "" && item.Post.FeaturedImage != null)
                {
                    media = "<a target='_blank' href='/Content/Article_Images/" + item.Post.FeaturedImage + "'>" + item.Post.FeaturedImage + "</a>";
                }

                commentsprofile.Add(new DataItem
                {
                    ItemName = item.CommentId.ToString(),
                    Description = TrimTextHelper.TrimText(item.CommentContent, 50),
                    CreationDate = item.CommentCreationDate.Value.ToShortDateString(),
                    ActivePosts = item.Post.Title,
                    PostContent = item.Post.Content,
                    Media = media,
                    Email = _userRepo.GetUserByUserId(item.CommentUserId.Value).UserName,
                    ReportedCommentsCount = item.CommentReportCount.ToString(),
                    DT_RowId = item.CommentId.ToString()
                });
            }

            dataTableData.data = commentsprofile;
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);

        }

        [CustomAuthorize(Roles = "Admin")]
        public void ExportComments(string status, string fromDate, string toDate, string search, int? PostId)
        {
            List<DataItem> data = new List<DataItem>();
            XLWorkbook wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("RWS Comments");
            string fileNamePrefix = "";

            ws.Cell("B2").Value = "Raya W Soffara " + status + " Comments";

            fileNamePrefix = "RWS_Comments_";
            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != null && fromDate != "")
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != null && toDate != "")
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }
            IQueryable<Comment> comments;
            comments = _postRepo.GetComments(null,PostId, search, status, from, to, null);

            foreach (var item in comments)
            {
                string media = "";
                if (item.Post.FeaturedVideo != "" && item.Post.FeaturedVideo != null)
                {
                    media = "https://www.youtube.com/watch?v=" + item.Post.FeaturedVideo;
                }
                else if (item.Post.FeaturedImage != "" || item.Post.FeaturedImage != null)
                {
                    media = Request.Url.GetLeftPart(UriPartial.Authority) + item.Post.FeaturedImage;
                }

                data.Add(new DataItem
                {
                    Description = item.CommentContent,
                    CreationDate = item.CommentCreationDate.Value.ToShortDateString(),
                    ActivePosts = item.Post.Title,
                    PostContent = TrimTextHelper.TrimText(item.Post.Content, 50),
                    Media = media,
                    Email = _userRepo.GetUserByUserId(item.CommentUserId.Value).UserName,
                    ReportedCommentsCount = item.CommentReportCount.ToString(),
                    DT_RowId = item.CommentId.ToString()
                });
            }

            ws.Cell("C3").Value = "From:  " + from;
            ws.Cell("D3").Value = "To:  " + to;

            ws.Cell("A4").Value = "Id";
            ws.Cell("B4").Value = "Content";
            ws.Cell("C4").Value = "Creation Date";
            ws.Cell("D4").Value = "Post Title";
            ws.Cell("E4").Value = "Post Content";
            ws.Cell("F4").Value = "Post Media";
            ws.Cell("G4").Value = "Username";
            ws.Cell("H4").Value = "Reports Count";
            int rowNum = 5;

            foreach (var item in data)
            {
                ws.Cell("A" + rowNum).Value = item.DT_RowId;
                ws.Cell("B" + rowNum).Value = item.Description;
                ws.Cell("C" + rowNum).Value = item.CreationDate;
                ws.Cell("D" + rowNum).Value = item.ActivePosts;
                ws.Cell("E" + rowNum).Value = item.PostContent;
                ws.Cell("F" + rowNum).Value = item.Media;
                ws.Cell("G" + rowNum).Value = item.Email;
                ws.Cell("H" + rowNum).Value = item.ReportedCommentsCount;
                rowNum++;
            }

            // From worksheet
            var rngTable = ws.Range("B2:C6");

            var rngHeaders = ws.Range("B2:C2"); // The address is relative to rngTable (NOT the worksheet)
            rngHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngHeaders.Style.Font.Bold = true;
            rngHeaders.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;

            ws.Range("A3:H3").Style.Font.Bold = true;
            ws.Range("A3:H3").Style.Fill.BackgroundColor = XLColor.Aqua;
            ws.Range("A3:H3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range("A4:H4").Style.Fill.BackgroundColor = XLColor.BlizzardBlue;

            ws.Row(1).Merge(); // We could've also used: rngTable.Range("A1:E1").Merge()
            ws.Row(2).Merge();

            ws.Columns(2, 6).AdjustToContents();

            string fileName = Server.UrlEncode(fileNamePrefix + DateTime.UtcNow.ToLocalTime().ToShortDateString().Replace("/", "_") + ".xlsx");
            MemoryStream stream = GetStream(wb);

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/vnd.ms-excel";
            Response.BinaryWrite(stream.ToArray());
            Response.End();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Advertisements()
        {
            ViewBag.SubSidebarItem = "advertisements";
            ViewBag.SidebarItem = "reports-management";
            ViewBag.PageHeader = "Advertisement Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetAdvertisements(int draw, int start, int length, string fromDate, string toDate)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";

            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != "" && fromDate != null)
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != "" && toDate != null)
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }

            int total_rows = _tutRepo.GetAdvertisements(null, from, to, search, null).Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> advertisements = new List<DataItem>();
            IQueryable<Advertisement> advs = _tutRepo.GetAdvertisements(start, from, to, search, length);
            foreach (var item in advs)
            {
                advertisements.Add(new DataItem
                {
                    Email = item.AdvertisementUserEmail,
                    ItemName = item.AdvertisementText,
                    CreationDate = item.AdvertisementTimestamp.Value.ToShortDateString(),
                    DT_RowId = item.AdvertisementId.ToString()
                });
            }
            dataTableData.data = advertisements;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public void ExportAdvertisements(string fromDate, string toDate, string search)
        {
            List<DataItem> data = new List<DataItem>();
            XLWorkbook wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("RWS Advertisements");
            string fileNamePrefix = "";

            ws.Cell("B2").Value = "Raya W Soffara Advertisements";

            fileNamePrefix = "RWS_Ads_";
            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != null && fromDate != "")
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != null && toDate != "")
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }
            IQueryable<Advertisement> ads;
            ads = _tutRepo.GetAdvertisements(null, from, to, search, null);

            foreach (var item in ads)
            {
                string creation = "";
                if (item.AdvertisementTimestamp != null)
                {
                    creation = item.AdvertisementTimestamp.Value.ToShortDateString();
                }

                data.Add(new DataItem
                {
                    Email = item.AdvertisementUserEmail,
                    Description = item.AdvertisementText,
                    CreationDate = creation,
                    DT_RowId = item.AdvertisementId.ToString()
                });
            }

            ws.Cell("B3").Value = "From:  " + from;
            ws.Cell("C3").Value = "To:  " + to;

            ws.Cell("A4").Value = "Id";
            ws.Cell("B4").Value = "User Email";
            ws.Cell("C4").Value = "Content";
            ws.Cell("D4").Value = "Timestamp";
            int rowNum = 5;

            foreach (var item in data)
            {
                ws.Cell("A" + rowNum).Value = item.DT_RowId;
                ws.Cell("B" + rowNum).Value = item.Email;
                ws.Cell("C" + rowNum).Value = item.Description;
                ws.Cell("D" + rowNum).Value = item.CreationDate;
                rowNum++;
            }

            // From worksheet
            var rngTable = ws.Range("B2:C6");

            var rngHeaders = ws.Range("B2:C2"); // The address is relative to rngTable (NOT the worksheet)
            rngHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngHeaders.Style.Font.Bold = true;
            rngHeaders.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;

            ws.Range("A3:D3").Style.Font.Bold = true;
            ws.Range("A3:D3").Style.Fill.BackgroundColor = XLColor.Aqua;
            ws.Range("A3:D3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range("A4:D4").Style.Fill.BackgroundColor = XLColor.BlizzardBlue;

            ws.Row(1).Merge(); // We could've also used: rngTable.Range("A1:E1").Merge()
            ws.Row(2).Merge();

            ws.Columns(2, 6).AdjustToContents();

            string fileName = Server.UrlEncode(fileNamePrefix + DateTime.UtcNow.ToLocalTime().ToShortDateString().Replace("/", "_") + ".xlsx");
            MemoryStream stream = GetStream(wb);

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/vnd.ms-excel";
            Response.BinaryWrite(stream.ToArray());
            Response.End();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Suggestions()
        {
            ViewBag.SubSidebarItem = "suggestions";
            ViewBag.SidebarItem = "reports-management";
            ViewBag.PageHeader = "Suggestions Management";
            return View();
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult AjaxGetSuggestions(int draw, int start, int length, string fromDate, string toDate)
        {
            string search = Request.Form["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";

            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != "" && fromDate != null)
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != "" && toDate != null)
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }

            int total_rows = _tutRepo.GetSuggestions(null, from, to, search, null).Count();
            if (length == -1)
            {
                length = total_rows;
            }

            // note: we only sort one column at a time
            if (Request.QueryString["order[0][column]"] != null)
            {
                sortColumn = int.Parse(Request.QueryString["order[0][column]"]);
            }
            if (Request.QueryString["order[0][dir]"] != null)
            {
                sortDirection = Request.QueryString["order[0][dir]"];
            }

            DataTableData dataTableData = new DataTableData();
            dataTableData.draw = draw;
            dataTableData.recordsTotal = total_rows;
            int recordsFiltered = total_rows;
            List<DataItem> suggestions = new List<DataItem>();
            IQueryable<Suggestion> sugg_items = _tutRepo.GetSuggestions(start, from, to, search, length);
            foreach (var item in sugg_items)
            {
                suggestions.Add(new DataItem
                {
                    Email = item.SuggestionUserEmail,
                    ItemName = item.SuggestionText,
                    CreationDate = item.SuggestionTimestamp.Value.ToShortDateString(),
                    DT_RowId = item.SuggestionId.ToString()
                });
            }
            dataTableData.data = suggestions;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        [CustomAuthorize(Roles = "Admin")]
        public void ExportSuggestions(string fromDate, string toDate, string search)
        {
            List<DataItem> data = new List<DataItem>();
            XLWorkbook wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("RWS Suggestions");
            string fileNamePrefix = "";

            ws.Cell("B2").Value = "Raya W Soffara Suggestions";

            fileNamePrefix = "RWS_Suggestions_";
            DateTime? from = null;
            DateTime? to = null;
            string[] from_date, to_date;
            if (fromDate != null && fromDate != "")
            {
                from_date = fromDate.Split('/');
                from = new DateTime(Int32.Parse(from_date[2]), Int32.Parse(from_date[0]), Int32.Parse(from_date[1]));
            }
            if (toDate != null && toDate != "")
            {
                to_date = toDate.Split('/');
                to = new DateTime(Int32.Parse(to_date[2]), Int32.Parse(to_date[0]), Int32.Parse(to_date[1]));
            }
            IQueryable<Suggestion> suggestions;
            suggestions = _tutRepo.GetSuggestions(null, from, to, search, null);

            foreach (var item in suggestions)
            {
                string creation = "";
                if (item.SuggestionTimestamp != null)
                {
                    creation = item.SuggestionTimestamp.Value.ToShortDateString();
                }

                data.Add(new DataItem
                {
                    Email = item.SuggestionUserEmail,
                    Description = item.SuggestionText,
                    CreationDate = creation,
                    DT_RowId = item.SuggestionId.ToString()
                });
            }

            ws.Cell("B3").Value = "From:  " + from;
            ws.Cell("C3").Value = "To:  " + to;

            ws.Cell("A4").Value = "Id";
            ws.Cell("B4").Value = "User Email";
            ws.Cell("C4").Value = "Content";
            ws.Cell("D4").Value = "Timestamp";
            int rowNum = 5;

            foreach (var item in data)
            {
                ws.Cell("A" + rowNum).Value = item.DT_RowId;
                ws.Cell("B" + rowNum).Value = item.Email;
                ws.Cell("C" + rowNum).Value = item.Description;
                ws.Cell("D" + rowNum).Value = item.CreationDate;
                rowNum++;
            }

            // From worksheet
            var rngTable = ws.Range("B2:C6");

            var rngHeaders = ws.Range("B2:C2"); // The address is relative to rngTable (NOT the worksheet)
            rngHeaders.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rngHeaders.Style.Font.Bold = true;
            rngHeaders.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;

            ws.Range("A3:D3").Style.Font.Bold = true;
            ws.Range("A3:D3").Style.Fill.BackgroundColor = XLColor.Aqua;
            ws.Range("A3:D3").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range("A4:D4").Style.Fill.BackgroundColor = XLColor.BlizzardBlue;

            ws.Row(1).Merge(); // We could've also used: rngTable.Range("A1:E1").Merge()
            ws.Row(2).Merge();

            ws.Columns(2, 6).AdjustToContents();

            string fileName = Server.UrlEncode(fileNamePrefix + DateTime.UtcNow.ToLocalTime().ToShortDateString().Replace("/", "_") + ".xlsx");
            MemoryStream stream = GetStream(wb);

            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            Response.ContentType = "application/vnd.ms-excel";
            Response.BinaryWrite(stream.ToArray());
            Response.End();
        }
        #endregion
    }
}