using RWSDataLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RWSDataLayer.Interfaces;

namespace RayaWSoffara.Models
{
    public class PostPointsVM
    {
        public int PostId { get; set; }
        public string PostTitle { get; set; }
        public string PostFeaturedImage { get; set; }
        public string PostFeaturedVideo { get; set; }

        public int PostSharesCount { get; set; }
        public int PostLikesCount { get; set; }
        public int PostViewsCount { get; set; }
        public double PostSharesValue { get; set; }
        public double PostLikesValue { get; set; }
        public double PostViewsValue { get; set; }
    }
}
