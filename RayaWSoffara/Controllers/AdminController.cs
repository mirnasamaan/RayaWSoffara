using System;
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

namespace RayaWSoffara.Controllers
{
    public class AdminController : Controller
    {
        UserRepository _userRepo = new UserRepository();
        CompetitionRepository _compRepo = new CompetitionRepository();
        ArticleRepository _postRepo = new ArticleRepository();

        //[CustomAuthorize(Roles = "Admin")]
        //public ActionResult Index(RWSUser user)
        //{
        //    return View();
        //}

        public ActionResult Signin(string RedirectUrl)
        {
            ViewBag.RedirectUrl = RedirectUrl;
            return View("Signin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignIn(string username, string password, string RedirectUrl)
        {
            RWSUser user = _userRepo.GetUserByUsernameAndPassword(username, password);
            bool userValid = _userRepo.UserExists(username, password);
            if (userValid)
            {
                password = GetMd5Hash(password);
                //var result = FormsAuthentication.Authenticate(username, password);
                FormsAuthentication.SetAuthCookie(username, true);
                return Redirect("/Admin/Users");
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
            public int articlesCount { get; set; }
            public string DT_RowId { get; set; }
            public string Status { get; set; }
            public string Actions { get; set; }
            public string Featured { get; set; }
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

        public ActionResult Signout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Signin", "Admin");
        }

        #region Users
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Users()
        {
            ViewBag.SidebarItem = "users-management";
            ViewBag.PageHeader = "Users Management";
            ViewBag.SubSidebarItem = "users";
            return View();
        }

        public ActionResult AjaxGetUsers(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            var watch = Stopwatch.StartNew();
            int total_rows = _userRepo.GetAllUsers().Count();
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
            int recordsFiltered = 0;
            int displayedNum;
            List<DataItem> userprofiles = new List<DataItem>();
            watch = Stopwatch.StartNew();
            IQueryable<RWSUser> users = _userRepo.GetAllUsers(start, length, out displayedNum);
            watch.Stop();
            var elapsedMS2 = watch.ElapsedMilliseconds;
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

                userprofiles.Add(new DataItem { ItemName = item.UserName, articlesCount = item.Posts.Where(i => i.IsActive == true).Count(), Actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>", DT_RowId = item.UserName, Status = status });
            }
            dataTableData.data = userprofiles;
            //dataTableData.data = FilterData(ref recordsFiltered, start, length, search, sortColumn, sortDirection);
            dataTableData.recordsFiltered = recordsFiltered;

            return Json(dataTableData, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteUser(string Username)
        {
            RWSUser user = _userRepo.GetUserByUsername(Username);
            _userRepo.DeleteUser(user);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

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
        public ActionResult AddUser(RWSUser user, string roleName)
        {
            RWSRole role = _userRepo.GetRoleByName(roleName);
            user.RWSRoles.Clear();
            user.RWSRoles.Add(role);
            user.Password = GetMd5Hash(user.Password);
            _userRepo.AddUser(user);
            ViewBag.AddSuccess = true;
            return View("Users");
        }

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
        public ActionResult DeactivateUser(string username)
        {
            _userRepo.DeactivateUser(username);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
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
            Region reg = _compRepo.AddRegion(region);
            Tag tag = new Tag();
            tag.TagName = region.RegionName;
            tag.TagType = 1;
            tag.TagTypeId = reg.RegionId;
            _postRepo.AddTag(tag);
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.Now.ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + region.RegionName + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Region_Logos"), fileName);
                path.SaveAs(localpath);
                reg.RegionIcon = "/Content/Region_Logos/" + fileName;
                _compRepo.UpdateRegion(reg);
            }
            return Redirect("/Admin/Regions");
        }

        public ActionResult AjaxGetRegions(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            var watch = Stopwatch.StartNew();
            int total_rows = _compRepo.GetAllRegions().Count();
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
            List<DataItem> regionprofiles = new List<DataItem>();
            watch = Stopwatch.StartNew();
            IQueryable<Region> regions = _compRepo.GetAllRegions(start, length);
            watch.Stop();
            var elapsedMS2 = watch.ElapsedMilliseconds;
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
                string time = DateTime.Now.ToString("yyyymmddhhmmssfff");
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
            string[] teamIds = teams.Split(',');
            foreach (var item in teamIds)
            {
                competition.Teams.Add(_compRepo.GetTeamById(Int32.Parse(item)));
            }
            Competition comp = _compRepo.AddCompetition(competition);
            Tag tag = new Tag();
            tag.TagName = comp.CompetitionName;
            tag.TagType = 2;
            tag.TagTypeId = comp.CompetitionId;
            _postRepo.AddTag(tag);
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.Now.ToString("yyyymmddhhmmssfff");
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
            IQueryable<Competition> competitions = _compRepo.GetAllCompetetions(start, length);
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
                string time = DateTime.Now.ToString("yyyymmddhhmmssfff");
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
            Team db_team = _compRepo.AddTeam(team, compIds);
            if (players != "")
            {
                List<int> playerIds = players.Split(',').Select(s => int.Parse(s)).ToList();
                _compRepo.AddTeamPlayers(db_team, playerIds);
            }
            Tag tag = new Tag();
            tag.TagName = db_team.TeamName;
            tag.TagType = 3;
            tag.TagTypeId = db_team.TeamId;
            _postRepo.AddTag(tag);
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.Now.ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + db_team.TeamName + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Team_Logos"), fileName);
                path.SaveAs(localpath);
                db_team.TeamIcon = "/Content/Team_Logos/" + fileName;
                _compRepo.UpdateTeam(db_team, compIds);
            }
            return Redirect("/Admin/Teams");
        }

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
            IQueryable<Team> teams = _compRepo.GetAllTeams(start, length);
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
                string time = DateTime.Now.ToString("yyyymmddhhmmssfff");
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
            Player db_player = _compRepo.AddPlayer(player, teamIds);

            Tag tag = new Tag();
            tag.TagName = db_player.PlayerName;
            tag.TagType = 4;
            tag.TagTypeId = db_player.PlayerId;
            _postRepo.AddTag(tag);
            if (path != null && path.ContentLength > 0)
            {
                string extention = Path.GetExtension(path.FileName);
                string time = DateTime.Now.ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + db_player.PlayerName + extention;
                var localpath = Path.Combine(Server.MapPath("/Content/Player_Logos"), fileName);
                path.SaveAs(localpath);
                db_player.PlayerIcon = "/Content/Player_Logos/" + fileName;
                _compRepo.UpdatePlayer(db_player, teamIds);
            }
            return Redirect("/Admin/Players");
        }

        public ActionResult AjaxGetPlayers(int draw, int start, int length)
        {
            string search = Request.QueryString["search[value]"];
            int sortColumn = -1;
            string sortDirection = "asc";
            var watch = Stopwatch.StartNew();
            int total_rows = _compRepo.GetAllPlayers().Count();
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
            List<DataItem> playerplrofiles = new List<DataItem>();
            watch = Stopwatch.StartNew();
            IQueryable<Player> players = _compRepo.GetAllPlayers(start, length);
            watch.Stop();
            var elapsedMS2 = watch.ElapsedMilliseconds;
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
                string time = DateTime.Now.ToString("yyyymmddhhmmssfff");
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
            IQueryable<Tag> tags = _postRepo.GetAllTags(start, length);
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

        public ActionResult RemoveTagAsFeatured(int TagId)
        {
            _postRepo.RemoveTagAsFeatured(TagId);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

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
            return View(images);
        }

        [CustomAuthorize(Roles = "Admin")]
        public ActionResult DeleteImage(int ImageId)
        {
            Image image = _compRepo.GetImageById(ImageId);
            _compRepo.DeleteImage(image);
            return Json(true, JsonRequestBehavior.AllowGet);
        }

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
                string time = DateTime.Now.ToString("yyyymmddhhmmssfff");
                string fileName = time + "_" + file.FileName;
                var localpath = Path.Combine(Server.MapPath("/Content/Article_Images"), fileName);
                file.SaveAs(localpath);
                Image image = new Image();
                image.ImageURL = "/Content/Article_Images/" + fileName;
                _compRepo.AddImage(image);
            }
            return Redirect("/Admin/Images");
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

        //[CustomAuthorize(Roles = "Admin")]
        //public ActionResult GetPlayers()
        //{
        //    List<Item> players = new List<Item>();
        //    IQueryable<Player> db_players = _compRepo.GetAllPlayers();
        //    foreach (var item in db_players)
        //    {
        //        players.Add(new Item() { ItemId = item.PlayerId, ItemName = item.PlayerName });
        //    }
        //    return Json(players, JsonRequestBehavior.AllowGet);
        //}
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
                string status;
                if (item.IsActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                string actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";

                posts.Add(new DataItem { ItemName = item.Title, Actions = actions, Status = status, DT_RowId = item.PostId.ToString() });
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
            article.newArticle.CreationDate = DateTime.Now;
            article.newArticle.MetaTags = "";
            List<Tag> tags = _postRepo.getSelectedTags(article.SelectedTags).ToList();
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
                        string imgName = DateTime.Now.Ticks + "_" + temp[1];
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

            article.newArticle.Tags = null;
            article.newArticle.MetaTags = "";
            article.newArticle.ViewsCount = 0;
            article.newArticle.SharesCount = 0;
            article.newArticle.PostTypeId = 1;
            Post addedArticle = _postRepo.AddPost(article.newArticle);
            _postRepo.UpdatedArticleTags(article.newArticle.PostId, tags);
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
                            string imgName = DateTime.Now.Ticks + "_" + temp[1];
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

        #region Opinions
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Opinions()
        {
            ViewBag.SubSidebarItem = "opinions";
            ViewBag.SidebarItem = "posts-management";
            ViewBag.PageHeader = "Posts Management";
            return View();
        }

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
                string status;
                if (item.IsActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                string actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";

                posts.Add(new DataItem { ItemName = item.Content, Actions = actions, Status = status, DT_RowId = item.PostId.ToString() });
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
            article.newArticle.CreationDate = DateTime.Now;
            article.newArticle.MetaTags = "";
            List<Tag> tags = _postRepo.getSelectedTags(article.SelectedTags).ToList();
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
                        string imgName = DateTime.Now.Ticks + "_" + temp[1];
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

            article.newArticle.Tags = null;
            article.newArticle.MetaTags = "";
            article.newArticle.ViewsCount = 0;
            article.newArticle.SharesCount = 0;
            article.newArticle.PostTypeId = 3;
            Post addedArticle = _postRepo.AddPost(article.newArticle);
            _postRepo.UpdatedArticleTags(article.newArticle.PostId, tags);
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
                            string imgName = DateTime.Now.Ticks + "_" + temp[1];
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
                string status;
                if (item.IsActive == true)
                {
                    status = "<span onclick='Deactivate(this)' class='status-action label label-success'>Active</span>";
                }
                else
                {
                    status = "<span onclick='Activate(this)' class='status-action label label-danger'>Inactive</span>";
                }
                string actions = "<a href='#' onclick='Edit(this);return false;'<i class='fa fa-pencil'></i></a><a href='#' onclick='Delete(this);return false;'<i class='fa fa-trash-o'></i></a>";
                string imageHtml = "<img src='/Content/Article_Images/" + item.FeaturedImage + "?w=230&h=140&mode=crop' />";
                posts.Add(new DataItem { ItemName = imageHtml, Actions = actions, Status = status, DT_RowId = item.PostId.ToString() });
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
            article.newArticle.CreationDate = DateTime.Now;
            article.newArticle.MetaTags = "";
            List<Tag> tags = _postRepo.getSelectedTags(article.SelectedTags).ToList();
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
                        string imgName = DateTime.Now.Ticks + "_" + temp[1];
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

            article.newArticle.Tags = null;
            article.newArticle.MetaTags = "";
            article.newArticle.ViewsCount = 0;
            article.newArticle.SharesCount = 0;
            article.newArticle.PostTypeId = 3;
            Post addedArticle = _postRepo.AddPost(article.newArticle);
            _postRepo.UpdatedArticleTags(article.newArticle.PostId, tags);
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
        public ActionResult EditImagePost(int PostId)
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
        public ActionResult EditImagePost(UserArticleVM articleVM, string article_picture_path, string video_url)
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
                            string imgName = DateTime.Now.Ticks + "_" + temp[1];
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
    }
}