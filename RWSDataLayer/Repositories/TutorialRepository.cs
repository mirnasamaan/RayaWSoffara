using RWSDataLayer.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace RWSDataLayer.Repositories
{
    public class TutorialRepository : BaseRepository<Tutorial>
    {
        public Tutorial GetTutorialById(int TutorialId)
        {
            return Context.Tutorials.FirstOrDefault(i => i.TutId == TutorialId);
        }

        public UserTutorial GetUserTutorialByTutAndUserId(int TutorialId, int UserId)
        {
            return Context.UserTutorials.FirstOrDefault(i => i.TutId == TutorialId && i.UserId == UserId);
        }

        public bool UserTutorialExist(int TutorialId, int UserId)
        {
            UserTutorial userTut = Context.UserTutorials.FirstOrDefault(i => i.TutId == TutorialId && i.UserId == UserId);
            if (userTut != null)
            {
                return true;
            }
            return false;
        }

        public bool ViewTutorial(int TutorialId, int UserId) 
        {
           try
           {
               if (UserTutorialExist(TutorialId, UserId) == false)
               {
                   UserTutorial userTut = new UserTutorial();
                   userTut.TutId = TutorialId;
                   userTut.UserId = UserId;
                   userTut.isViewed = true;
                   Context.UserTutorials.Add(userTut);
               }
               else
               {
                   UserTutorial userTut = Context.UserTutorials.FirstOrDefault(i => i.TutId == TutorialId && i.UserId == UserId);
                   userTut.isViewed = true;
               }
               Context.SaveChanges();
               return true;
           }
           catch (Exception ex)
           {
               return false;
           }
        }
    }
}
