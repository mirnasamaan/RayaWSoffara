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
    
    public partial class Tutorial
    {
        public Tutorial()
        {
            this.UserTutorials = new HashSet<UserTutorial>();
        }
    
        public int TutId { get; set; }
        public string TutTitle { get; set; }
        public string TutScript { get; set; }
    
        public virtual ICollection<UserTutorial> UserTutorials { get; set; }
    }
}
