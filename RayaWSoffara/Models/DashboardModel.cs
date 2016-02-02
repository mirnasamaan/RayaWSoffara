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
        public int CleanComments { get; set; }
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
    }
}
