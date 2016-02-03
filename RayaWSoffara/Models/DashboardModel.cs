using System;
using System.ComponentModel.DataAnnotations;

namespace RayaWSoffara.Models
{
    public class Sidebar
    {
        public int ActiveUsers { get; set; }
        public int PendingUsers { get; set; }
        public int ActivePosts { get; set; }
        public int PendingPosts { get; set; }
        public int NonReportedComments { get; set; }
        public int ReportedComments { get; set; }
    }

    public class Dashboard
    {
        public int RegisterdUsersThisMonth { get; set; }
        public int ActivatedUsersThisMonth { get; set; }
        public int PendingUsersThisMonth { get; set; }
        public int RegisteredUsersToday { get; set; }
        public int ActivatedUsersToday { get; set; }
        public int PendingUsersToday { get; set; }

        public int TotalPostsThisMonth { get; set; }
        public int ActivatedPostsThisMonth { get; set; }
        public int PendingPostsThisMonth { get; set; }
        public int TotalPostsToday { get; set; }
        public int ActivatedPostsToday { get; set; }
        public int PendingPostsToday { get; set; }

        public int TotalCommentsThisMonth { get; set; }
        public int NonReportedCommentsThisMonth { get; set; }
        public int ReportedCommentsThisMonth { get; set; }
        public int TotalCommentsToday { get; set; }
        public int NonreportedCommentsToday { get; set; }
        public int ReportedCommentsToday { get; set; }
    }
}
