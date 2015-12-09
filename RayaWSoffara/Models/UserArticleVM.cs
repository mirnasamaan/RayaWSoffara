using RWSDataLayer.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RayaWSoffara.Models
{
    public class UserArticleVM
    {
        public UserArticleVM()
        {
            currentUser = new RWSUser();
            newArticle = new Post();
            newTopX = new List<ArticleTopX>();
            Tags = new List<Tag>();
            SelectedTags = new List<string>();
            userArticles = new List<Post>();
            Comments = new List<Comment>();
        }
        public RWSUser currentUser { get; set; }
        public Post newArticle { get; set; }
        public List<ArticleTopX> newTopX { get; set; }
        public List<Tag> Tags { get; set; }
        public List<string> SelectedTags { get; set; }
        public List<Post> userArticles { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
