using RWSDataLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWSDataLayer.Interfaces;

namespace RayaWSoffara.Models
{
    public class UserPointsDetailsVM
    {
        public int UserId { get; set; }
        public string UserName { get; set;  }
        public int MonthId { get; set;  }
        public string MonthName { get; set; }
        public int YearId { get; set; }
        public double TotalPointsValue { get; set; }

        public List<PostPointsVM> PostsPoints { get; set; }
    }
}
