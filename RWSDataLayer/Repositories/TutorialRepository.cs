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
    }
}
