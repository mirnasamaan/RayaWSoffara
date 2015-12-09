using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace RayaWSoffara.Models
{
    public class CompetitionItem
    {
        public int CompetitionId { get; set; }
        public string CompetitionName { get; set; }
    }

    public class TeamItem
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
    }
}
