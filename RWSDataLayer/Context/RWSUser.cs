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
    
    public partial class RWSUser
    {
        public RWSUser()
        {
            this.Comments = new HashSet<Comment>();
            this.Posts = new HashSet<Post>();
            this.RWSProviderUsers = new HashSet<RWSProviderUser>();
            this.CommentReports = new HashSet<CommentReport>();
            this.Points = new HashSet<Point>();
            this.UserTutorials = new HashSet<UserTutorial>();
            this.RWSRoles = new HashSet<RWSRole>();
        }
    
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string Country { get; set; }
        public string Email { get; set; }
        public string ProfileImagePath { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<System.DateTime> ConfirmationDate { get; set; }
        public Nullable<System.DateTime> BanDate { get; set; }
        public Nullable<bool> IsConfirmed { get; set; }
        public Nullable<bool> IsBanned { get; set; }
        public string ConfirmationToken { get; set; }
        public Nullable<System.DateTime> LastPasswordFailureDate { get; set; }
        public Nullable<int> PasswordFaliuresSinceLastSuccess { get; set; }
        public Nullable<System.DateTime> PasswordChangedDate { get; set; }
        public string PasswordSalt { get; set; }
        public string PasswordVerificationToken { get; set; }
        public Nullable<System.DateTime> PasswordVerificationTokenExpirationDate { get; set; }
        public Nullable<int> FavComp { get; set; }
        public Nullable<int> FavTeam { get; set; }
        public string AboutYou { get; set; }
        public string Twitter { get; set; }
        public string Google { get; set; }
        public string DisplayName { get; set; }
    
        public virtual ICollection<Comment> Comments { get; set; }
        public virtual Competition Competition { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        public virtual ICollection<RWSProviderUser> RWSProviderUsers { get; set; }
        public virtual ICollection<CommentReport> CommentReports { get; set; }
        public virtual ICollection<Point> Points { get; set; }
        public virtual Team Team { get; set; }
        public virtual ICollection<UserTutorial> UserTutorials { get; set; }
        public virtual ICollection<RWSRole> RWSRoles { get; set; }
    }
}
