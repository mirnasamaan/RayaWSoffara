using System;
using System.ComponentModel.DataAnnotations;

namespace RayaWSoffara.Models
{
    public class Dashboard
    {
        public int AllTimeRegisteredUsers { get; set; }
        public int AllTimeActiveUsers { get; set; }
        public int AllTimePendingUsers { get; set; }
        public int AllTimePosts { get; set; }
        public int AllTimeActivePosts { get; set; }
        public int AllTimePendingPosts { get; set; }
        public int AllTimeComments { get; set; }
        public int AllTimeNonReportedComments { get; set; }
        public int AllTimeReportedComments { get; set; }

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
