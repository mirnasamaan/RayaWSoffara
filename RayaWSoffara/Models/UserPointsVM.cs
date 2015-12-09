using RWSDataLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWSDataLayer.Interfaces;

namespace RayaWSoffara.Models
{
    public class UserPointsVM
    {
        public int UserId { get; set; }
        public string UserName { get; set;  }
        public string UserProfilePicture { get; set; }
        public int MonthId { get; set;  }
        public string MonthName { get; set; }
        public int YearId { get; set; }
        public int SharesCount { get; set; }
        public int LikesCount { get; set; }
        public int ViewsCount { get; set; }
        public double SharesValue { get; set; }
        public double LikesValue { get; set; }
        public double ViewsValue { get; set; }
        public double PointsValue { get; set; }
    }
}
