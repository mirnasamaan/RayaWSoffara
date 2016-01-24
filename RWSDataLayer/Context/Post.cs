//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace RWSDataLayer.Context
{
    using System;
    using System.Collections.Generic;
    
    public partial class Post
    {
        public Post()
        {
            this.ArticleTopXes = new HashSet<ArticleTopX>();
            this.Engagements = new HashSet<Engagement>();
            this.Tags = new HashSet<Tag>();
            this.Comments = new HashSet<Comment>();
        }
    
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string MetaTags { get; set; }
        public string FeaturedVideo { get; set; }
        public string FeaturedImage { get; set; }
        public int ViewsCount { get; set; }
        public Nullable<bool> HasImage { get; set; }
        public Nullable<bool> IsFeatured { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<System.DateTime> ActivationDate { get; set; }
        public Nullable<System.DateTime> UpdateDate { get; set; }
        public Nullable<int> PostTypeId { get; set; }
        public int SharesCount { get; set; }
        public int LikesCount { get; set; }
        public Nullable<bool> isDeleted { get; set; }
    
        public virtual ICollection<ArticleTopX> ArticleTopXes { get; set; }
        public virtual PostType PostType { get; set; }
        public virtual RWSUser RWSUser { get; set; }
        public virtual ICollection<Engagement> Engagements { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
