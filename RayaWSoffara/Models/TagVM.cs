using RWSDataLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayaWSoffara.Models
{
    public class TagVM
    {
        public int TagID { get; set; }
        public string TagName { get; set; }
    }

    public class PostTypeVM
    {
        public int PostTypeID { get; set; }
        public string PostTypeName { get; set; }
    }

    public class PostVM
    {
        public int PostID { get; set; }
        public int PostTypeID { get; set; }
        public string PostTypeName { get; set; }
        public int PostAuthorID { get; set; }
        public string PostAuthorName { get; set; }
        public string PostAuthorProfilePicture { get; set; }
        public int PostViewsCount { get; set; }
        public int PostSharesCount { get; set; }
        public int PostCommentsCount { get; set; }
        public string PostAuthorUsername { get; set; }
        public string PostTitle { get; set; }
        public string PostImage { get; set; }
        public string PostVideo { get; set; }
        public string PostContent { get; set; }
        public DateTime ActivationDate { get; set; }
    }

    public class IndexVM
    {
        public IndexVM() { 
            Posts = new List<PostVM>();
        }
        public int PostsCount { get; set; }
        public List<PostVM> Posts;
    }
}
