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


namespace RayaWSoffara.Controllers
{
    [Authorize]
    public class CompetitionController : Controller
    {
        public IFormsAuthenticationService FormsService { get; set; }
        public IMembershipService MembershipService { get; set; }
        private CompetitionRepository _compRepo;

        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult GetAllCompetetions()
        {
            _compRepo = new CompetitionRepository();
            IQueryable<Competition> comps = _compRepo.GetAllCompetetions();
            List<CompetitionItem> result = new List<CompetitionItem>();
            foreach (var item in comps)
            {
                result.Add(new CompetitionItem { CompetitionId = item.CompetitionId, CompetitionName = item.CompetitionName });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult GetTeamsForCompetition(string compId)
        {
            _compRepo = new CompetitionRepository();
            IQueryable<Team> teams = _compRepo.GetTeamsForCompetition(Int32.Parse(compId));
            List<TeamItem> result = new List<TeamItem>();
            foreach (var item in teams)
            {
                result.Add(new TeamItem { TeamId = item.TeamId, TeamName = item.TeamName });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
